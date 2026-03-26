using System;
using System.Collections.Generic;

namespace TTA.Core
{
    [Serializable]
    public enum PlacementState
    {
        Unplaced = 0,
        Placed = 1
    }

    [Serializable]
    public enum MatchExecutionMode
    {
        Setup = 0,
        WaitingForPlayerAction = 1,
        WaitingForReaction = 2,
        Resolving = 3,
        Ended = 4
    }

    [Serializable]
    public sealed class RuntimeRandomState
    {
        public int seed = 1;
        public int state = 1;
    }

    [Serializable]
    public sealed class MatchProgressionState
    {
        public string rulesetKey = string.Empty;
        public string currentPhaseKey = string.Empty;
        public int currentPlayerId = RuntimeIds.InvalidId;
        public bool ended;
        public int winnerPlayerId = RuntimeIds.InvalidId;
    }

    [Serializable]
    public sealed class EventPayload
    {
        public string trigger = string.Empty;
        public ValueMap fields;
        public bool hasMovementData;
        public int movementElementId = RuntimeIds.InvalidId;
        public int movementRequestedSteps;
        public int movementActualSteps;
        public int movementAreaId = RuntimeIds.InvalidId;
        public int movementFinalAreaId = RuntimeIds.InvalidId;
        public string movementTopologyKey = string.Empty;
        public string movementLinkName = string.Empty;
    }

    [Serializable]
    public sealed class MatchExecutionState
    {
        public MatchExecutionMode mode = MatchExecutionMode.Setup;
        public string queuedNextPhase = string.Empty;
        public int resolvingPlayerId = RuntimeIds.InvalidId;
        public bool hasCurrentEvent;
        public EventPayload currentEvent = new();
        public List<EventPayload> queuedEvents = new();
    }

    [Serializable]
    public sealed class RuntimePlayerRecord
    {
        public int id = RuntimeIds.InvalidId;
        public int orderIndex = RuntimeIds.InvalidIndex;
        public ValueMap properties = new();
        public ValueMap temps = new();
    }

    [Serializable]
    public sealed class RuntimeElementRecord
    {
        public int id = RuntimeIds.InvalidId;
        public int definitionIndex = RuntimeIds.InvalidIndex;
        public int ownerPlayerId = RuntimeIds.InvalidId;
        public PlacementState placementState = PlacementState.Unplaced;
        public int areaId = RuntimeIds.InvalidId;
        public int slotId = RuntimeIds.InvalidId;
        public int orderIndex = RuntimeIds.InvalidIndex;
        public int currentFaceIndex = RuntimeIds.InvalidIndex;
        public ValueMap properties = new();
        public ValueMap temps = new();
    }

    [Serializable]
    public sealed class RuntimeAreaRecord
    {
        public int id = RuntimeIds.InvalidId;
        public int definitionIndex = RuntimeIds.InvalidIndex;
        public int ownerElementId = RuntimeIds.InvalidId;
        public ValueMap properties = new();
        public ValueMap temps = new();
        public List<int> slotIds = new();
    }

    [Serializable]
    public sealed class RuntimeSlotRecord
    {
        public int id = RuntimeIds.InvalidId;
        public int areaId = RuntimeIds.InvalidId;
        public int definitionIndex = RuntimeIds.InvalidIndex;
        public List<int> elementIds = new();
    }

    [Serializable]
    public sealed class RuntimeTopologyLinkRecord
    {
        public int fromAreaId = RuntimeIds.InvalidId;
        public int toAreaId = RuntimeIds.InvalidId;
        public string name = string.Empty;
    }

    [Serializable]
    public sealed class RuntimeTopologyRecord
    {
        public string key = string.Empty;
        public int ownerElementId = RuntimeIds.InvalidId;
        public List<RuntimeTopologyLinkRecord> links = new();
    }

    [Serializable]
    public sealed class RuntimePlayerContainer
    {
        public List<RuntimePlayerRecord> items = new();
    }

    [Serializable]
    public sealed class RuntimeElementContainer
    {
        public List<RuntimeElementRecord> items = new();
    }

    [Serializable]
    public sealed class RuntimeAreaContainer
    {
        public List<RuntimeAreaRecord> items = new();
    }

    [Serializable]
    public sealed class RuntimeSlotContainer
    {
        public List<RuntimeSlotRecord> items = new();
    }

    [Serializable]
    public sealed class RuntimeTopologyContainer
    {
        public List<RuntimeTopologyRecord> items = new();
    }

    [Serializable]
    public sealed class BoxStockEntry
    {
        public int elementDefinitionIndex = RuntimeIds.InvalidIndex;
        public int availableCount;
    }

    [Serializable]
    public sealed class BoxStockState
    {
        public List<BoxStockEntry> entries = new();
    }

    [Serializable]
    public sealed class MatchState
    {
        public MatchProgressionState progression = new();
        public MatchExecutionState execution = new();
        public MatchInteractionState interaction = new();
        public MatchTranscriptState transcript = new();
        public MatchHistoryState history = new();
        public RuntimePlayerContainer players = new();
        public RuntimeElementContainer elements = new();
        public RuntimeAreaContainer areas = new();
        public RuntimeSlotContainer slots = new();
        public RuntimeTopologyContainer topologies = new();
        public ValueMap properties = new();
        public MatchTempState temps = new();
        public RuntimeIdCounters idCounters = new();
        public BoxStockState boxStock = new();
        public RuntimeRandomState random = new();
    }
}
