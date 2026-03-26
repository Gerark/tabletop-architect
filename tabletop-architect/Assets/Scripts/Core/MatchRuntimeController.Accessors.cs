using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed partial class MatchRuntimeController
    {
        private enum AreaDefinitionValidationError
        {
            None = 0,
            DuplicateKey = 1,
            InvalidDefaultSlotCount = 2
        }

        private bool TryValidateAreaDefinitions(AreaDefinition[] areas, out AreaDefinitionValidationError error, out string areaKey)
        {
            error = AreaDefinitionValidationError.None;
            areaKey = string.Empty;
            Dictionary<string, bool> seenKeys = new(StringComparer.Ordinal);
            for (int areaIndex = 0; areaIndex < areas.Length; areaIndex++)
            {
                AreaDefinition area = areas[areaIndex];
                string key = area.key ?? string.Empty;
                if (seenKeys.ContainsKey(key))
                {
                    error = AreaDefinitionValidationError.DuplicateKey;
                    areaKey = key;
                    return false;
                }

                seenKeys.Add(key, true);

                if (area.slots == null || area.slots.Length == 0)
                    continue;

                int defaultSlots = 0;
                for (int slotIndex = 0; slotIndex < area.slots.Length; slotIndex++)
                {
                    if (area.slots[slotIndex].isDefault)
                        defaultSlots++;
                }

                if (area.slots.Length > 1 && defaultSlots != 1)
                {
                    error = AreaDefinitionValidationError.InvalidDefaultSlotCount;
                    areaKey = key;
                    return false;
                }
            }

            return true;
        }

        private void ValidateTopologyDefinitions(ElementDefinition elementDefinition)
        {
            Dictionary<string, bool> areaKeys = new(StringComparer.Ordinal);
            for (int areaIndex = 0; areaIndex < elementDefinition.ownedAreas.Length; areaIndex++)
                areaKeys[elementDefinition.ownedAreas[areaIndex].key ?? string.Empty] = true;

            Dictionary<string, bool> topologyKeys = new(StringComparer.Ordinal);
            for (int topologyIndex = 0; topologyIndex < elementDefinition.topologies.Length; topologyIndex++)
            {
                TopologyDefinition topology = elementDefinition.topologies[topologyIndex];
                string topologyKey = topology.key ?? string.Empty;
                if (topologyKeys.ContainsKey(topologyKey))
                    throw new InvalidOperationException($"Duplicate topology key '{topologyKey}' in element '{elementDefinition.key}'.");

                topologyKeys[topologyKey] = true;

                for (int pathIndex = 0; pathIndex < topology.linearPaths.Length; pathIndex++)
                {
                    for (int nodeIndex = 0; nodeIndex < topology.linearPaths[pathIndex].areas.Length; nodeIndex++)
                    {
                        string areaKey = topology.linearPaths[pathIndex].areas[nodeIndex] ?? string.Empty;
                        if (!areaKeys.ContainsKey(areaKey))
                            throw new InvalidOperationException($"Topology '{topologyKey}' references unknown owned area '{areaKey}' in element '{elementDefinition.key}'.");
                    }
                }

                for (int groupIndex = 0; groupIndex < topology.linkGroups.Length; groupIndex++)
                {
                    for (int linkIndex = 0; linkIndex < topology.linkGroups[groupIndex].links.Length; linkIndex++)
                    {
                        TopologyLinkDefinition link = topology.linkGroups[groupIndex].links[linkIndex];
                        if (!areaKeys.ContainsKey(link.from ?? string.Empty) || !areaKeys.ContainsKey(link.to ?? string.Empty))
                        {
                            throw new InvalidOperationException($"Topology '{topologyKey}' references unknown owned area in element '{elementDefinition.key}'.");
                        }
                    }
                }
            }
        }

        private void ValidatePhaseKeys(RulesetDefinition ruleset)
        {
            Dictionary<string, bool> phaseKeys = new(StringComparer.Ordinal);
            for (int index = 0; index < ruleset.play.phases.Length; index++)
            {
                string key = ruleset.play.phases[index].key ?? string.Empty;
                if (phaseKeys.ContainsKey(key))
                    throw new InvalidOperationException($"Duplicate phase key '{key}' in ruleset '{ruleset.key}'.");

                phaseKeys.Add(key, true);
            }
        }

        private void InitializeMatchProperties(MatchState match)
        {
            for (int index = 0; index < _definition.properties.Length; index++)
            {
                PropertyDefinition property = _definition.properties[index];
                if (property.scope == PropertyScope.Match)
                    match.properties.Set(property.key, GetDefaultValue(property));
            }
        }

        private void InitializeBoxStock(MatchState match)
        {
            for (int index = 0; index < _definition.elements.Length; index++)
            {
                match.boxStock.entries.Add(new BoxStockEntry
                {
                    elementDefinitionIndex = index,
                    availableCount = Math.Max(0, _definition.elements[index].amount)
                });
            }
        }

        private void CreatePlayers(MatchState match, int playerCount)
        {
            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
            {
                RuntimePlayerRecord player = new()
                {
                    id = match.idCounters.nextPlayerId++,
                    orderIndex = playerIndex
                };

                for (int propertyIndex = 0; propertyIndex < _definition.properties.Length; propertyIndex++)
                {
                    PropertyDefinition property = _definition.properties[propertyIndex];
                    if (property.scope == PropertyScope.Player)
                        player.properties.Set(property.key, GetDefaultValue(property));
                }

                match.players.items.Add(player);
            }
        }

        private void CreateGlobalAreas(MatchState match)
        {
            for (int areaIndex = 0; areaIndex < _definition.globalAreas.Length; areaIndex++)
                CreateRuntimeArea(match, RuntimeIds.InvalidId, areaIndex);
        }

        private RulesetDefinition GetRulesetDefinition(string rulesetKey)
        {
            if (string.IsNullOrWhiteSpace(rulesetKey))
            {
                if (_definition.rulesets.Length == 1)
                    return _definition.rulesets[0];

                throw new InvalidOperationException("A ruleset key is required when multiple rulesets exist.");
            }

            if (!_rulesetIndicesByKey.TryGetValue(rulesetKey, out int rulesetIndex))
                throw new InvalidOperationException($"Ruleset '{rulesetKey}' was not found.");

            return _definition.rulesets[rulesetIndex];
        }

        private PhaseDefinition GetCurrentPhaseDefinition(MatchState match)
        {
            return GetPhaseDefinition(GetRulesetDefinition(match.progression.rulesetKey), match.progression.currentPhaseKey);
        }

        private PhaseDefinition GetPhaseDefinition(RulesetDefinition ruleset, string phaseKey)
        {
            for (int index = 0; index < ruleset.play.phases.Length; index++)
            {
                if (string.Equals(ruleset.play.phases[index].key, phaseKey, StringComparison.Ordinal))
                    return ruleset.play.phases[index];
            }

            throw new InvalidOperationException($"Phase '{phaseKey}' was not found in ruleset '{ruleset.key}'.");
        }

        private ElementDefinition GetElementDefinition(RuntimeElementRecord element)
        {
            return _definition.elements[element.definitionIndex];
        }

        private AreaDefinition GetAreaDefinition(MatchState match, RuntimeAreaRecord area)
        {
            if (area.ownerElementId == RuntimeIds.InvalidId)
                return _definition.globalAreas[area.definitionIndex];

            RuntimeElementRecord owner = match.GetElement(area.ownerElementId);
            return GetElementDefinition(owner).ownedAreas[area.definitionIndex];
        }

        private AreaDefinition GetAreaDefinition(MatchState match, int ownerElementId, int definitionIndex)
        {
            if (ownerElementId == RuntimeIds.InvalidId)
                return _definition.globalAreas[definitionIndex];

            RuntimeElementRecord owner = match.GetElement(ownerElementId);
            return GetElementDefinition(owner).ownedAreas[definitionIndex];
        }

        private string GetRuntimeAreaKey(MatchState match, int areaId)
        {
            return GetAreaDefinition(match, match.GetArea(areaId)).key;
        }

        private SlotDefinition GetSlotDefinition(MatchState match, RuntimeSlotRecord slot)
        {
            RuntimeAreaRecord area = match.GetArea(slot.areaId);
            AreaDefinition areaDefinition = GetAreaDefinition(match, area);
            if (slot.definitionIndex == RuntimeIds.InvalidIndex)
                return CreateSyntheticDefaultSlot();

            return areaDefinition.slots[slot.definitionIndex];
        }

        private SlotDefinition CreateSyntheticDefaultSlot()
        {
            return new SlotDefinition
            {
                key = "default",
                isDefault = true,
                capacityKind = SlotCapacityKind.Multiple,
                capacityLimit = 0
            };
        }

        private SlotDefinition[] GetEffectiveSlotDefinitions(AreaDefinition area)
        {
            return area.slots == null || area.slots.Length == 0
                ? new[] { CreateSyntheticDefaultSlot() }
                : area.slots;
        }

        private int GetDefaultSlotId(MatchState match, int areaId)
        {
            RuntimeAreaRecord area = match.GetArea(areaId);
            AreaDefinition definition = GetAreaDefinition(match, area);

            if (definition.slots == null || definition.slots.Length == 0)
                return area.slotIds[0];

            if (definition.slots.Length == 1)
                return area.slotIds[0];

            for (int index = 0; index < definition.slots.Length; index++)
            {
                if (definition.slots[index].isDefault)
                    return area.slotIds[index];
            }

            throw new InvalidOperationException($"Area '{definition.key}' does not have a default slot.");
        }

        private RuntimeAreaRecord CreateRuntimeArea(MatchState match, int ownerElementId, int definitionIndex)
        {
            MatchHistoryTimeline.TrackIdCounters(match);
            AreaDefinition definition = GetAreaDefinition(match, ownerElementId, definitionIndex);
            RuntimeAreaRecord area = new()
            {
                id = match.idCounters.nextAreaId++,
                definitionIndex = definitionIndex,
                ownerElementId = ownerElementId
            };

            for (int propertyIndex = 0; propertyIndex < definition.properties.Length; propertyIndex++)
                area.properties.Set(definition.properties[propertyIndex].key, GetDefaultValue(definition.properties[propertyIndex]));

            match.areas.items.Add(area);
            MatchHistoryTimeline.TrackAreaAdded(match, area.id);

            SlotDefinition[] slotDefinitions = GetEffectiveSlotDefinitions(definition);
            for (int slotIndex = 0; slotIndex < slotDefinitions.Length; slotIndex++)
            {
                RuntimeSlotRecord slot = new()
                {
                    id = match.idCounters.nextSlotId++,
                    areaId = area.id,
                    definitionIndex = definition.slots == null || definition.slots.Length == 0 ? RuntimeIds.InvalidIndex : slotIndex
                };

                match.slots.items.Add(slot);
                MatchHistoryTimeline.TrackSlotAdded(match, slot.id);
                area.slotIds.Add(slot.id);
            }

            return area;
        }

        private RuntimeElementRecord CreateRuntimeElement(MatchState match, int definitionIndex, int ownerPlayerId)
        {
            MatchHistoryTimeline.TrackIdCounters(match);
            ElementDefinition definition = _definition.elements[definitionIndex];
            RuntimeElementRecord element = new()
            {
                id = match.idCounters.nextElementId++,
                definitionIndex = definitionIndex,
                ownerPlayerId = ownerPlayerId,
                currentFaceIndex = GetInitialFaceIndex(definition)
            };

            for (int propertyIndex = 0; propertyIndex < definition.properties.Length; propertyIndex++)
                element.properties.Set(definition.properties[propertyIndex].key, GetDefaultValue(definition.properties[propertyIndex]));

            return element;
        }

        private Value GetDefaultValue(PropertyDefinition definition)
        {
            if (definition.defaultValue != null && !definition.defaultValue.IsNull)
                return definition.defaultValue.DeepCopy();

            return definition.valueKind switch
            {
                ValueKind.Int => Value.FromInt(0),
                ValueKind.Float => Value.FromFloat(0f),
                ValueKind.Bool => Value.FromBool(false),
                ValueKind.String => Value.FromString(string.Empty),
                ValueKind.ElementId => Value.FromElementId(RuntimeIds.InvalidId),
                ValueKind.PlayerId => Value.FromPlayerId(RuntimeIds.InvalidId),
                ValueKind.AreaId => Value.FromAreaId(RuntimeIds.InvalidId),
                ValueKind.SlotId => Value.FromSlotId(RuntimeIds.InvalidId),
                ValueKind.Collection => Value.FromCollection(Array.Empty<Value>()),
                _ => Value.Null()
            };
        }

        private void EnsureValueMatchesKind(Value value, ValueKind expectedKind)
        {
            if (value.kind != expectedKind)
                throw new InvalidOperationException($"Value kind '{value.kind}' does not match expected kind '{expectedKind}'.");
        }

        private void ValidatePropertyWrite(PropertyScope scope, string key, Value value)
        {
            for (int index = 0; index < _definition.properties.Length; index++)
            {
                PropertyDefinition property = _definition.properties[index];
                if (property.scope == scope && string.Equals(property.key, key, StringComparison.Ordinal))
                {
                    EnsureValueMatchesKind(value, property.valueKind);
                    return;
                }
            }

            throw new InvalidOperationException($"Property '{key}' is not defined for scope '{scope}'.");
        }

        private void ValidateElementPropertyWrite(MatchState match, int elementId, string key, Value value)
        {
            ElementDefinition definition = GetElementDefinition(match.GetElement(elementId));
            for (int index = 0; index < definition.properties.Length; index++)
            {
                if (!string.Equals(definition.properties[index].key, key, StringComparison.Ordinal))
                    continue;

                EnsureValueMatchesKind(value, definition.properties[index].valueKind);
                return;
            }

            throw new InvalidOperationException($"Property '{key}' is not defined for element '{definition.key}'.");
        }

        private void ValidateAreaPropertyWrite(MatchState match, int areaId, string key, Value value)
        {
            AreaDefinition definition = GetAreaDefinition(match, match.GetArea(areaId));
            for (int index = 0; index < definition.properties.Length; index++)
            {
                if (!string.Equals(definition.properties[index].key, key, StringComparison.Ordinal))
                    continue;

                EnsureValueMatchesKind(value, definition.properties[index].valueKind);
                return;
            }

            throw new InvalidOperationException($"Property '{key}' is not defined for area '{definition.key}'.");
        }

        private bool ElementHasTag(ElementDefinition definition, string tag)
        {
            for (int index = 0; index < definition.tags.Length; index++)
            {
                if (string.Equals(definition.tags[index], tag, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private int GetInitialFaceIndex(ElementDefinition definition)
        {
            if (definition.faces.Length == 0)
                return RuntimeIds.InvalidIndex;

            for (int index = 0; index < definition.faces.Length; index++)
            {
                if (definition.faces[index].isDefault)
                    return index;
            }

            return 0;
        }

        private int FindFaceIndex(ElementDefinition definition, string faceId)
        {
            for (int index = 0; index < definition.faces.Length; index++)
            {
                if (string.Equals(definition.faces[index].id, faceId, StringComparison.Ordinal))
                    return index;
            }

            throw new InvalidOperationException($"Face '{faceId}' was not found on element '{definition.key}'.");
        }

        private int NextRandomIndex(MatchState match, int maxExclusive)
        {
            MatchHistoryTimeline.TrackRandom(match);
            long nextState = (1103515245L * match.random.state + 12345L) & 0x7fffffffL;
            match.random.state = (int)nextState;
            return maxExclusive <= 0 ? 0 : match.random.state % maxExclusive;
        }

        private void QueueEvent(MatchState match, EventPayload payload)
        {
            MatchHistoryTimeline.TrackExecution(match);
            match.execution.queuedEvents.Add(payload);
            RecordEventQueued(match, payload);
        }

        private int IndexOf(List<int> values, int value)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (values[index] == value)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        private void EnsureUniqueIds(List<int> ids)
        {
            HashSet<int> seenIds = new();
            for (int index = 0; index < ids.Count; index++)
            {
                if (!seenIds.Add(ids[index]))
                    throw new InvalidOperationException($"Duplicate runtime id '{ids[index]}' is not supported in a single atomic operation.");
            }
        }
    }
}
