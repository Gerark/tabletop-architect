using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed partial class MatchRuntimeController
    {
        private void PlaceElementIds(MatchState match, List<int> elementIds, int areaId, int slotId)
        {
            if (elementIds.Count == 0)
                throw new InvalidOperationException("PlaceElement requires at least one runtime element.");

            EnsureUniqueIds(elementIds);

            RuntimeAreaRecord area = match.GetArea(areaId);
            RuntimeSlotRecord destinationSlot = match.GetSlot(slotId);
            if (destinationSlot.areaId != area.id)
                throw new InvalidOperationException("The selected destination slot does not belong to the selected area.");

            SlotDefinition destinationSlotDefinition = GetSlotDefinition(match, destinationSlot);
            ValidatePlacementCapacity(match, elementIds, destinationSlot, destinationSlotDefinition);

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.placementState == PlacementState.Unplaced)
                    ValidateOwnedContentCreation(GetElementDefinition(element));
            }

            List<int> sourceSlotIds = new();
            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.placementState == PlacementState.Placed && !sourceSlotIds.Contains(element.slotId))
                    sourceSlotIds.Add(element.slotId);
            }

            for (int sourceIndex = 0; sourceIndex < sourceSlotIds.Count; sourceIndex++)
            {
                RuntimeSlotRecord sourceSlot = match.GetSlot(sourceSlotIds[sourceIndex]);
                for (int index = sourceSlot.elementIds.Count - 1; index >= 0; index--)
                {
                    if (elementIds.Contains(sourceSlot.elementIds[index]))
                        sourceSlot.elementIds.RemoveAt(index);
                }

                RefreshSlotOrderIndices(match, sourceSlot);
            }

            bool destinationAlreadyContainsSameSingle =
                destinationSlotDefinition.capacityKind == SlotCapacityKind.Single &&
                destinationSlot.elementIds.Count == 1 &&
                elementIds.Count == 1 &&
                destinationSlot.elementIds[0] == elementIds[0];

            if (!destinationAlreadyContainsSameSingle)
            {
                for (int index = 0; index < elementIds.Count; index++)
                    destinationSlot.elementIds.Add(elementIds[index]);
            }

            RefreshSlotOrderIndices(match, destinationSlot);

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                bool wasUnplaced = element.placementState == PlacementState.Unplaced;

                element.placementState = PlacementState.Placed;
                element.areaId = area.id;
                element.slotId = destinationSlot.id;
                element.orderIndex = IndexOf(destinationSlot.elementIds, element.id);

                if (wasUnplaced)
                    CreateOwnedRuntimeContent(match, element.id);
            }
        }

        private void ValidatePlacementCapacity(MatchState match, List<int> elementIds, RuntimeSlotRecord destinationSlot, SlotDefinition slotDefinition)
        {
            int selectedElementsAlreadyInDestination = 0;
            for (int index = 0; index < destinationSlot.elementIds.Count; index++)
            {
                if (elementIds.Contains(destinationSlot.elementIds[index]))
                    selectedElementsAlreadyInDestination++;
            }

            if (slotDefinition.capacityKind == SlotCapacityKind.Single)
            {
                bool sameSingleReorder =
                    elementIds.Count == 1 &&
                    destinationSlot.elementIds.Count == 1 &&
                    destinationSlot.elementIds[0] == elementIds[0];

                if (!sameSingleReorder)
                {
                    if (elementIds.Count != 1)
                        throw new InvalidOperationException("Single-capacity slots only accept one element.");

                    if (destinationSlot.elementIds.Count > 0)
                        throw new InvalidOperationException("Single-capacity destination slot is already occupied.");
                }

                return;
            }

            if (slotDefinition.capacityLimit > 0)
            {
                int projectedCount = destinationSlot.elementIds.Count - selectedElementsAlreadyInDestination + elementIds.Count;
                if (projectedCount > slotDefinition.capacityLimit)
                    throw new InvalidOperationException("Destination slot does not have enough capacity for the requested batch.");
            }
        }

        private void RefreshSlotOrderIndices(MatchState match, RuntimeSlotRecord slot)
        {
            for (int index = 0; index < slot.elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(slot.elementIds[index]);
                element.orderIndex = index;
            }
        }

        private void RemoveElementFromSlot(MatchState match, RuntimeSlotRecord slot, int elementId)
        {
            int index = IndexOf(slot.elementIds, elementId);
            if (index >= 0)
                slot.elementIds.RemoveAt(index);

            RefreshSlotOrderIndices(match, slot);
        }

        private void ValidateOwnedContentCreation(ElementDefinition definition)
        {
            ValidateTopologyDefinitions(definition);
            ValidateAreaDefinitions(definition.ownedAreas, $"element '{definition.key}'");
        }

        private void EnsureOwnedContentEmpty(MatchState match, int ownerElementId)
        {
            for (int areaIndex = 0; areaIndex < match.areas.items.Count; areaIndex++)
            {
                RuntimeAreaRecord area = match.areas.items[areaIndex];
                if (area.ownerElementId != ownerElementId)
                    continue;

                for (int slotIndex = 0; slotIndex < area.slotIds.Count; slotIndex++)
                {
                    RuntimeSlotRecord slot = match.GetSlot(area.slotIds[slotIndex]);
                    if (slot.elementIds.Count > 0)
                        throw new InvalidOperationException("Elements with occupied owned areas cannot be unplaced or returned to the box.");
                }
            }
        }

        private void CreateOwnedRuntimeContent(MatchState match, int ownerElementId)
        {
            RuntimeElementRecord owner = match.GetElement(ownerElementId);
            ElementDefinition definition = GetElementDefinition(owner);
            if (definition.ownedAreas.Length == 0 && definition.topologies.Length == 0)
                return;

            Dictionary<string, int> createdAreaIds = new(StringComparer.Ordinal);
            for (int areaIndex = 0; areaIndex < definition.ownedAreas.Length; areaIndex++)
            {
                RuntimeAreaRecord area = CreateRuntimeArea(match, ownerElementId, areaIndex);
                createdAreaIds.Add(definition.ownedAreas[areaIndex].key ?? string.Empty, area.id);
            }

            for (int topologyIndex = 0; topologyIndex < definition.topologies.Length; topologyIndex++)
            {
                TopologyDefinition topology = definition.topologies[topologyIndex];
                RuntimeTopologyRecord runtimeTopology = new()
                {
                    key = topology.key,
                    ownerElementId = ownerElementId
                };

                for (int pathIndex = 0; pathIndex < topology.linearPaths.Length; pathIndex++)
                {
                    LinearPathDefinition linearPath = topology.linearPaths[pathIndex];
                    for (int areaIndex = 0; areaIndex < linearPath.areas.Length - 1; areaIndex++)
                    {
                        runtimeTopology.links.Add(new RuntimeTopologyLinkRecord
                        {
                            fromAreaId = createdAreaIds[linearPath.areas[areaIndex] ?? string.Empty],
                            toAreaId = createdAreaIds[linearPath.areas[areaIndex + 1] ?? string.Empty],
                            name = "Forward"
                        });
                    }

                    if (linearPath.loop && linearPath.areas.Length > 1)
                    {
                        runtimeTopology.links.Add(new RuntimeTopologyLinkRecord
                        {
                            fromAreaId = createdAreaIds[linearPath.areas[linearPath.areas.Length - 1] ?? string.Empty],
                            toAreaId = createdAreaIds[linearPath.areas[0] ?? string.Empty],
                            name = "Forward"
                        });
                    }
                }

                for (int groupIndex = 0; groupIndex < topology.linkGroups.Length; groupIndex++)
                {
                    TopologyLinkGroupDefinition group = topology.linkGroups[groupIndex];
                    for (int linkIndex = 0; linkIndex < group.links.Length; linkIndex++)
                    {
                        runtimeTopology.links.Add(new RuntimeTopologyLinkRecord
                        {
                            fromAreaId = createdAreaIds[group.links[linkIndex].from ?? string.Empty],
                            toAreaId = createdAreaIds[group.links[linkIndex].to ?? string.Empty],
                            name = group.links[linkIndex].name ?? string.Empty
                        });
                    }
                }

                match.topologies.items.Add(runtimeTopology);
            }
        }

        private void DestroyOwnedRuntimeContent(MatchState match, int ownerElementId)
        {
            for (int topologyIndex = match.topologies.items.Count - 1; topologyIndex >= 0; topologyIndex--)
            {
                if (match.topologies.items[topologyIndex].ownerElementId == ownerElementId)
                    match.topologies.items.RemoveAt(topologyIndex);
            }

            for (int areaIndex = match.areas.items.Count - 1; areaIndex >= 0; areaIndex--)
            {
                RuntimeAreaRecord area = match.areas.items[areaIndex];
                if (area.ownerElementId != ownerElementId)
                    continue;

                for (int slotIndex = area.slotIds.Count - 1; slotIndex >= 0; slotIndex--)
                {
                    int slotListIndex = match.GetSlotIndex(area.slotIds[slotIndex]);
                    match.slots.items.RemoveAt(slotListIndex);
                }

                match.areas.items.RemoveAt(areaIndex);
            }
        }
    }
}
