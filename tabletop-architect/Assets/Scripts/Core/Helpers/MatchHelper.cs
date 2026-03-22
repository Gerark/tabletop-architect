using System;

namespace TTA.Core
{
    public static class MatchHelper
    {
        public static RuntimePlayerRecord GetPlayer(this MatchState match, int playerId)
        {
            for (int index = 0; index < match.players.items.Count; index++)
            {
                if (match.players.items[index].id == playerId)
                    return match.players.items[index];
            }

            throw new InvalidOperationException($"Runtime player {playerId} was not found.");
        }

        public static RuntimeElementRecord GetElement(this MatchState match, int elementId)
        {
            for (int index = 0; index < match.elements.items.Count; index++)
            {
                if (match.elements.items[index].id == elementId)
                    return match.elements.items[index];
            }

            throw new InvalidOperationException($"Runtime element {elementId} was not found.");
        }

        public static RuntimeAreaRecord GetArea(this MatchState match, int areaId)
        {
            for (int index = 0; index < match.areas.items.Count; index++)
            {
                if (match.areas.items[index].id == areaId)
                    return match.areas.items[index];
            }

            throw new InvalidOperationException($"Runtime area {areaId} was not found.");
        }

        public static RuntimeSlotRecord GetSlot(this MatchState match, int slotId)
        {
            for (int index = 0; index < match.slots.items.Count; index++)
            {
                if (match.slots.items[index].id == slotId)
                    return match.slots.items[index];
            }

            throw new InvalidOperationException($"Runtime slot {slotId} was not found.");
        }

        public static RuntimeTopologyRecord GetRuntimeTopology(this MatchState match, string topologyKey, int ownerElementId)
        {
            for (int index = 0; index < match.topologies.items.Count; index++)
            {
                RuntimeTopologyRecord topology = match.topologies.items[index];
                if (topology.ownerElementId == ownerElementId && string.Equals(topology.key, topologyKey, StringComparison.Ordinal))
                    return topology;
            }

            throw new InvalidOperationException($"Runtime topology '{topologyKey}' was not found for owner '{ownerElementId}'.");
        }

        public static BoxStockEntry GetBoxStockEntry(this MatchState match, int definitionIndex)
        {
            for (int index = 0; index < match.boxStock.entries.Count; index++)
            {
                if (match.boxStock.entries[index].elementDefinitionIndex == definitionIndex)
                    return match.boxStock.entries[index];
            }

            throw new InvalidOperationException($"Box stock entry for definition index {definitionIndex} was not found.");
        }

        public static int GetElementIndex(this MatchState match, int elementId)
        {
            for (int index = 0; index < match.elements.items.Count; index++)
            {
                if (match.elements.items[index].id == elementId)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        public static int GetSlotIndex(this MatchState match, int slotId)
        {
            for (int index = 0; index < match.slots.items.Count; index++)
            {
                if (match.slots.items[index].id == slotId)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

    }
}