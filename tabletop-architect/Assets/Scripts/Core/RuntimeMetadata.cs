using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Core
{
    [Serializable]
    public enum InteractionWindowKind
    {
        None = 0,
        PlayerAction = 1,
        Reaction = 2
    }

    [Serializable]
    public sealed class InteractionWindow
    {
        public int id = RuntimeIds.InvalidId;
        public InteractionWindowKind kind = InteractionWindowKind.None;
        public int primaryPlayerId = RuntimeIds.InvalidId;
        public List<int> eligiblePlayerIds = new();
        public string phaseKey = string.Empty;
        public string sourceTrigger = string.Empty;
        public ValueMap metadata = new();
    }

    [Serializable]
    public sealed class MatchInteractionState
    {
        public InteractionWindow currentWindow = new();
        public int pendingActionPlayerId = RuntimeIds.InvalidId;
    }

    [Serializable]
    public enum TranscriptStopReason
    {
        None = 0,
        WaitingForPlayerAction = 1,
        WaitingForReaction = 2,
        MatchEnded = 3
    }

    [Serializable]
    public enum TranscriptEntryKind
    {
        Info = 0,
        ActionSubmitted = 1,
        ReactionSubmitted = 2,
        PhaseChanged = 3,
        EventQueued = 4,
        EventResolved = 5,
        WaitOpened = 6,
        ElementsTakenFromBox = 7,
        ElementsPlaced = 8,
        ElementsUnplaced = 9,
        ElementsReturnedToBox = 10,
        ElementMoved = 11,
        FaceChanged = 12,
        RollResolved = 13,
        TurnAdvanced = 14,
        MatchEnded = 15
    }

    [Serializable]
    public sealed class TranscriptPrivateField
    {
        public string key = string.Empty;
        public int visibleToPlayerId = RuntimeIds.InvalidId;
        public Value visibleValue = Value.Null();
        public Value hiddenValue = Value.Null();
    }

    [Serializable]
    public sealed class TranscriptEntry
    {
        public TranscriptEntryKind kind = TranscriptEntryKind.Info;
        public string code = string.Empty;
        public int actorPlayerId = RuntimeIds.InvalidId;
        public ValueMap fields = new();
        public List<TranscriptPrivateField> privateFields = new();
    }

    [Serializable]
    public sealed class TranscriptBatch
    {
        public int id = RuntimeIds.InvalidId;
        public int observerPlayerId = RuntimeIds.InvalidId;
        public TranscriptStopReason stopReason = TranscriptStopReason.None;
        public int interactionWindowId = RuntimeIds.InvalidId;
        public List<TranscriptEntry> entries = new();
        public ValueMap metadata = new();
    }

    [Serializable]
    public sealed class MatchTranscriptState
    {
        public int nextBatchId = RuntimeIds.FirstValidId;
        public List<TranscriptEntry> pendingEntries = new();
        public List<TranscriptBatch> completedBatches = new();
    }

    [Serializable]
    public sealed class MatchCheckpointMetadata
    {
        public int interactionWindowId = RuntimeIds.InvalidId;
        public MatchExecutionMode mode = MatchExecutionMode.Setup;
        public int actorPlayerId = RuntimeIds.InvalidId;
        public string phaseKey = string.Empty;
        public string sourceTrigger = string.Empty;
    }

    [Serializable]
    public sealed class MatchStateSnapshot
    {
        public MatchProgressionState progression = new();
        public MatchExecutionState execution = new();
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
        public MatchInteractionState interaction = new();
    }

    [Serializable]
    public sealed class MatchStateDelta
    {
        public bool progressionChanged;
        public MatchProgressionState progression = new();
        public bool executionChanged;
        public MatchExecutionState execution = new();
        public bool playersChanged;
        public RuntimePlayerContainer players = new();
        public bool elementsChanged;
        public RuntimeElementContainer elements = new();
        public bool areasChanged;
        public RuntimeAreaContainer areas = new();
        public bool slotsChanged;
        public RuntimeSlotContainer slots = new();
        public bool topologiesChanged;
        public RuntimeTopologyContainer topologies = new();
        public bool propertiesChanged;
        public ValueMap properties = new();
        public bool tempsChanged;
        public MatchTempState temps = new();
        public bool idCountersChanged;
        public RuntimeIdCounters idCounters = new();
        public bool boxStockChanged;
        public BoxStockState boxStock = new();
        public bool randomChanged;
        public RuntimeRandomState random = new();
        public bool interactionChanged;
        public MatchInteractionState interaction = new();
    }

    [Serializable]
    public sealed class MatchCheckpointRecord
    {
        public int id = RuntimeIds.InvalidId;
        public MatchCheckpointMetadata metadata = new();
        public MatchStateDelta undoDelta = new();
        public MatchStateDelta redoDelta = new();
    }

    [Serializable]
    public sealed class MatchHistoryState
    {
        public int nextCheckpointId = RuntimeIds.FirstValidId;
        public bool initialized;
        public int currentIndex;
        public MatchCheckpointMetadata baselineMetadata = new();
        public MatchStateSnapshot currentSnapshot = new();
        public List<MatchCheckpointRecord> checkpoints = new();
    }

    internal static class RuntimeDataClone
    {
        public static T Clone<T>(T value)
        {
            if (value == null)
                return default;

            string json = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool AreEqual<T>(T left, T right)
        {
            return string.Equals(
                JsonConvert.SerializeObject(left),
                JsonConvert.SerializeObject(right),
                StringComparison.Ordinal);
        }
    }

    internal static class MatchStateSnapshots
    {
        public static MatchStateSnapshot Capture(MatchState match)
        {
            return new MatchStateSnapshot
            {
                progression = RuntimeDataClone.Clone(match.progression) ?? new MatchProgressionState(),
                execution = RuntimeDataClone.Clone(match.execution) ?? new MatchExecutionState(),
                players = RuntimeDataClone.Clone(match.players) ?? new RuntimePlayerContainer(),
                elements = RuntimeDataClone.Clone(match.elements) ?? new RuntimeElementContainer(),
                areas = RuntimeDataClone.Clone(match.areas) ?? new RuntimeAreaContainer(),
                slots = RuntimeDataClone.Clone(match.slots) ?? new RuntimeSlotContainer(),
                topologies = RuntimeDataClone.Clone(match.topologies) ?? new RuntimeTopologyContainer(),
                properties = RuntimeDataClone.Clone(match.properties) ?? new ValueMap(),
                temps = RuntimeDataClone.Clone(match.temps) ?? new MatchTempState(),
                idCounters = RuntimeDataClone.Clone(match.idCounters) ?? new RuntimeIdCounters(),
                boxStock = RuntimeDataClone.Clone(match.boxStock) ?? new BoxStockState(),
                random = RuntimeDataClone.Clone(match.random) ?? new RuntimeRandomState(),
                interaction = RuntimeDataClone.Clone(match.interaction) ?? new MatchInteractionState()
            };
        }

        public static void Restore(MatchState match, MatchStateSnapshot snapshot)
        {
            match.progression = RuntimeDataClone.Clone(snapshot.progression) ?? new MatchProgressionState();
            match.execution = RuntimeDataClone.Clone(snapshot.execution) ?? new MatchExecutionState();
            match.players = RuntimeDataClone.Clone(snapshot.players) ?? new RuntimePlayerContainer();
            match.elements = RuntimeDataClone.Clone(snapshot.elements) ?? new RuntimeElementContainer();
            match.areas = RuntimeDataClone.Clone(snapshot.areas) ?? new RuntimeAreaContainer();
            match.slots = RuntimeDataClone.Clone(snapshot.slots) ?? new RuntimeSlotContainer();
            match.topologies = RuntimeDataClone.Clone(snapshot.topologies) ?? new RuntimeTopologyContainer();
            match.properties = RuntimeDataClone.Clone(snapshot.properties) ?? new ValueMap();
            match.temps = RuntimeDataClone.Clone(snapshot.temps) ?? new MatchTempState();
            match.idCounters = RuntimeDataClone.Clone(snapshot.idCounters) ?? new RuntimeIdCounters();
            match.boxStock = RuntimeDataClone.Clone(snapshot.boxStock) ?? new BoxStockState();
            match.random = RuntimeDataClone.Clone(snapshot.random) ?? new RuntimeRandomState();
            match.interaction = RuntimeDataClone.Clone(snapshot.interaction) ?? new MatchInteractionState();
        }

        public static MatchStateDelta CreateDelta(MatchStateSnapshot from, MatchStateSnapshot to)
        {
            MatchStateDelta delta = new();

            CopyIfChanged(from.progression, to.progression, ref delta.progressionChanged, ref delta.progression);
            CopyIfChanged(from.execution, to.execution, ref delta.executionChanged, ref delta.execution);
            CopyIfChanged(from.players, to.players, ref delta.playersChanged, ref delta.players);
            CopyIfChanged(from.elements, to.elements, ref delta.elementsChanged, ref delta.elements);
            CopyIfChanged(from.areas, to.areas, ref delta.areasChanged, ref delta.areas);
            CopyIfChanged(from.slots, to.slots, ref delta.slotsChanged, ref delta.slots);
            CopyIfChanged(from.topologies, to.topologies, ref delta.topologiesChanged, ref delta.topologies);
            CopyIfChanged(from.properties, to.properties, ref delta.propertiesChanged, ref delta.properties);
            CopyIfChanged(from.temps, to.temps, ref delta.tempsChanged, ref delta.temps);
            CopyIfChanged(from.idCounters, to.idCounters, ref delta.idCountersChanged, ref delta.idCounters);
            CopyIfChanged(from.boxStock, to.boxStock, ref delta.boxStockChanged, ref delta.boxStock);
            CopyIfChanged(from.random, to.random, ref delta.randomChanged, ref delta.random);
            CopyIfChanged(from.interaction, to.interaction, ref delta.interactionChanged, ref delta.interaction);

            return delta;
        }

        public static void Apply(MatchStateSnapshot snapshot, MatchStateDelta delta)
        {
            ApplyIfChanged(delta.progressionChanged, delta.progression, ref snapshot.progression);
            ApplyIfChanged(delta.executionChanged, delta.execution, ref snapshot.execution);
            ApplyIfChanged(delta.playersChanged, delta.players, ref snapshot.players);
            ApplyIfChanged(delta.elementsChanged, delta.elements, ref snapshot.elements);
            ApplyIfChanged(delta.areasChanged, delta.areas, ref snapshot.areas);
            ApplyIfChanged(delta.slotsChanged, delta.slots, ref snapshot.slots);
            ApplyIfChanged(delta.topologiesChanged, delta.topologies, ref snapshot.topologies);
            ApplyIfChanged(delta.propertiesChanged, delta.properties, ref snapshot.properties);
            ApplyIfChanged(delta.tempsChanged, delta.temps, ref snapshot.temps);
            ApplyIfChanged(delta.idCountersChanged, delta.idCounters, ref snapshot.idCounters);
            ApplyIfChanged(delta.boxStockChanged, delta.boxStock, ref snapshot.boxStock);
            ApplyIfChanged(delta.randomChanged, delta.random, ref snapshot.random);
            ApplyIfChanged(delta.interactionChanged, delta.interaction, ref snapshot.interaction);
        }

        private static void CopyIfChanged<T>(T from, T to, ref bool changed, ref T target)
        {
            if (RuntimeDataClone.AreEqual(from, to))
                return;

            changed = true;
            target = RuntimeDataClone.Clone(to);
        }

        private static void ApplyIfChanged<T>(bool changed, T value, ref T target)
        {
            if (!changed)
                return;

            target = RuntimeDataClone.Clone(value);
        }
    }

    internal static class MatchHistoryTimeline
    {
        public static void Capture(MatchState match, MatchCheckpointMetadata metadata)
        {
            MatchHistoryState history = match.history ??= new MatchHistoryState();
            MatchStateSnapshot snapshot = MatchStateSnapshots.Capture(match);

            if (!history.initialized)
            {
                history.initialized = true;
                history.currentIndex = 0;
                history.baselineMetadata = RuntimeDataClone.Clone(metadata) ?? new MatchCheckpointMetadata();
                history.currentSnapshot = RuntimeDataClone.Clone(snapshot) ?? new MatchStateSnapshot();
                return;
            }

            if (RuntimeDataClone.AreEqual(history.currentSnapshot, snapshot))
                return;

            TrimFuture(history);

            MatchCheckpointRecord record = new()
            {
                id = history.nextCheckpointId++,
                metadata = RuntimeDataClone.Clone(metadata) ?? new MatchCheckpointMetadata(),
                undoDelta = MatchStateSnapshots.CreateDelta(snapshot, history.currentSnapshot),
                redoDelta = MatchStateSnapshots.CreateDelta(history.currentSnapshot, snapshot)
            };

            history.checkpoints.Add(record);
            history.currentIndex = history.checkpoints.Count;
            history.currentSnapshot = RuntimeDataClone.Clone(snapshot) ?? new MatchStateSnapshot();
        }

        public static bool CanUndo(MatchState match)
        {
            return match != null &&
                match.history != null &&
                match.history.initialized &&
                match.history.currentIndex > 0;
        }

        public static bool CanRedo(MatchState match)
        {
            return match != null &&
                match.history != null &&
                match.history.initialized &&
                match.history.currentIndex < match.history.checkpoints.Count;
        }

        public static bool Undo(MatchState match)
        {
            if (!CanUndo(match))
                return false;

            MatchHistoryState history = match.history;
            MatchCheckpointRecord record = history.checkpoints[history.currentIndex - 1];
            MatchStateSnapshots.Apply(history.currentSnapshot, record.undoDelta);
            MatchStateSnapshots.Restore(match, history.currentSnapshot);
            history.currentIndex--;
            match.transcript.pendingEntries.Clear();
            return true;
        }

        public static bool Redo(MatchState match)
        {
            if (!CanRedo(match))
                return false;

            MatchHistoryState history = match.history;
            MatchCheckpointRecord record = history.checkpoints[history.currentIndex];
            MatchStateSnapshots.Apply(history.currentSnapshot, record.redoDelta);
            MatchStateSnapshots.Restore(match, history.currentSnapshot);
            history.currentIndex++;
            match.transcript.pendingEntries.Clear();
            return true;
        }

        private static void TrimFuture(MatchHistoryState history)
        {
            if (history.currentIndex >= history.checkpoints.Count)
                return;

            history.checkpoints.RemoveRange(history.currentIndex, history.checkpoints.Count - history.currentIndex);
        }
    }
}
