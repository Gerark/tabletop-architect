using System;
using System.Collections.Generic;

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
    public sealed class BoxStockEntryChange
    {
        public int definitionIndex = RuntimeIds.InvalidIndex;
        public int beforeCount;
        public int afterCount;
    }

    [Serializable]
    public sealed class RuntimePlayerRecordChange
    {
        public int id = RuntimeIds.InvalidId;
        public bool existedBefore;
        public bool existedAfter;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public int afterIndex = RuntimeIds.InvalidIndex;
        public RuntimePlayerRecord before;
        public RuntimePlayerRecord after;
    }

    [Serializable]
    public sealed class RuntimeElementRecordChange
    {
        public int id = RuntimeIds.InvalidId;
        public bool existedBefore;
        public bool existedAfter;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public int afterIndex = RuntimeIds.InvalidIndex;
        public RuntimeElementRecord before;
        public RuntimeElementRecord after;
    }

    [Serializable]
    public sealed class RuntimeAreaRecordChange
    {
        public int id = RuntimeIds.InvalidId;
        public bool existedBefore;
        public bool existedAfter;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public int afterIndex = RuntimeIds.InvalidIndex;
        public RuntimeAreaRecord before;
        public RuntimeAreaRecord after;
    }

    [Serializable]
    public sealed class RuntimeSlotRecordChange
    {
        public int id = RuntimeIds.InvalidId;
        public bool existedBefore;
        public bool existedAfter;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public int afterIndex = RuntimeIds.InvalidIndex;
        public RuntimeSlotRecord before;
        public RuntimeSlotRecord after;
    }

    [Serializable]
    public sealed class RuntimeTopologyRecordChange
    {
        public string key = string.Empty;
        public int ownerElementId = RuntimeIds.InvalidId;
        public bool existedBefore;
        public bool existedAfter;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public int afterIndex = RuntimeIds.InvalidIndex;
        public RuntimeTopologyRecord before;
        public RuntimeTopologyRecord after;
    }

    [Serializable]
    public sealed class MatchStateChangeSet
    {
        public bool progressionChanged;
        public MatchProgressionState progressionBefore;
        public MatchProgressionState progressionAfter;

        public bool executionChanged;
        public MatchExecutionState executionBefore;
        public MatchExecutionState executionAfter;

        public bool interactionChanged;
        public MatchInteractionState interactionBefore;
        public MatchInteractionState interactionAfter;

        public bool propertiesChanged;
        public ValueMap propertiesBefore;
        public ValueMap propertiesAfter;

        public bool tempsChanged;
        public MatchTempState tempsBefore;
        public MatchTempState tempsAfter;

        public bool idCountersChanged;
        public RuntimeIdCounters idCountersBefore;
        public RuntimeIdCounters idCountersAfter;

        public bool randomChanged;
        public RuntimeRandomState randomBefore;
        public RuntimeRandomState randomAfter;

        public List<BoxStockEntryChange> boxStockChanges;
        public List<RuntimePlayerRecordChange> playerChanges;
        public List<RuntimeElementRecordChange> elementChanges;
        public List<RuntimeAreaRecordChange> areaChanges;
        public List<RuntimeSlotRecordChange> slotChanges;
        public List<RuntimeTopologyRecordChange> topologyChanges;

        public bool HasAnyChanges()
        {
            return progressionChanged ||
                executionChanged ||
                interactionChanged ||
                propertiesChanged ||
                tempsChanged ||
                idCountersChanged ||
                randomChanged ||
                HasEntries(boxStockChanges) ||
                HasEntries(playerChanges) ||
                HasEntries(elementChanges) ||
                HasEntries(areaChanges) ||
                HasEntries(slotChanges) ||
                HasEntries(topologyChanges);
        }

        private static bool HasEntries<T>(List<T> entries)
        {
            return entries != null && entries.Count > 0;
        }
    }

    [Serializable]
    public sealed class MatchCheckpointRecord
    {
        public int id = RuntimeIds.InvalidId;
        public MatchCheckpointMetadata metadata = new();
        public MatchStateChangeSet changes = new();
    }

    [Serializable]
    public sealed class MatchHistoryState
    {
        public int nextCheckpointId = RuntimeIds.FirstValidId;
        public bool initialized;
        public int currentIndex;
        public MatchCheckpointMetadata baselineMetadata = new();
        public List<MatchCheckpointRecord> checkpoints = new();

        [NonSerialized]
        internal MatchPendingHistory pending = new();

        [NonSerialized]
        internal bool isApplyingHistory;
    }

    internal readonly struct RuntimeTopologyHandle : IEquatable<RuntimeTopologyHandle>
    {
        public RuntimeTopologyHandle(int ownerElementId, string key)
        {
            this.ownerElementId = ownerElementId;
            this.key = key ?? string.Empty;
        }

        public readonly int ownerElementId;
        public readonly string key;

        public bool Equals(RuntimeTopologyHandle other)
        {
            return ownerElementId == other.ownerElementId &&
                string.Equals(key, other.key, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeTopologyHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ownerElementId * 397) ^ StringComparer.Ordinal.GetHashCode(key ?? string.Empty);
            }
        }
    }

    internal sealed class PendingRecordChange<T>
    {
        public bool existedBefore;
        public int beforeIndex = RuntimeIds.InvalidIndex;
        public T before;
    }

    internal sealed class MatchPendingHistory
    {
        public bool hasProgressionBefore;
        public MatchProgressionState progressionBefore;

        public bool hasExecutionBefore;
        public MatchExecutionState executionBefore;

        public bool hasInteractionBefore;
        public MatchInteractionState interactionBefore;

        public bool hasPropertiesBefore;
        public ValueMap propertiesBefore;

        public bool hasTempsBefore;
        public MatchTempState tempsBefore;

        public bool hasIdCountersBefore;
        public RuntimeIdCounters idCountersBefore;

        public bool hasRandomBefore;
        public RuntimeRandomState randomBefore;

        public readonly Dictionary<int, int> boxStockCountsBefore = new();
        public readonly Dictionary<int, PendingRecordChange<RuntimePlayerRecord>> playerChanges = new();
        public readonly Dictionary<int, PendingRecordChange<RuntimeElementRecord>> elementChanges = new();
        public readonly Dictionary<int, PendingRecordChange<RuntimeAreaRecord>> areaChanges = new();
        public readonly Dictionary<int, PendingRecordChange<RuntimeSlotRecord>> slotChanges = new();
        public readonly Dictionary<RuntimeTopologyHandle, PendingRecordChange<RuntimeTopologyRecord>> topologyChanges = new();

        public bool HasTrackedChanges()
        {
            return hasProgressionBefore ||
                hasExecutionBefore ||
                hasInteractionBefore ||
                hasPropertiesBefore ||
                hasTempsBefore ||
                hasIdCountersBefore ||
                hasRandomBefore ||
                boxStockCountsBefore.Count > 0 ||
                playerChanges.Count > 0 ||
                elementChanges.Count > 0 ||
                areaChanges.Count > 0 ||
                slotChanges.Count > 0 ||
                topologyChanges.Count > 0;
        }

        public void Clear()
        {
            hasProgressionBefore = false;
            progressionBefore = null;

            hasExecutionBefore = false;
            executionBefore = null;

            hasInteractionBefore = false;
            interactionBefore = null;

            hasPropertiesBefore = false;
            propertiesBefore = null;

            hasTempsBefore = false;
            tempsBefore = null;

            hasIdCountersBefore = false;
            idCountersBefore = null;

            hasRandomBefore = false;
            randomBefore = null;

            boxStockCountsBefore.Clear();
            playerChanges.Clear();
            elementChanges.Clear();
            areaChanges.Clear();
            slotChanges.Clear();
            topologyChanges.Clear();
        }
    }

    internal static class MatchStateSnapshots
    {
        public static MatchStateSnapshot Capture(MatchState match)
        {
            return new MatchStateSnapshot
            {
                progression = RuntimeStateCopy.Clone(match.progression) ?? new MatchProgressionState(),
                execution = RuntimeStateCopy.Clone(match.execution) ?? new MatchExecutionState(),
                players = RuntimeStateCopy.Clone(match.players) ?? new RuntimePlayerContainer(),
                elements = RuntimeStateCopy.Clone(match.elements) ?? new RuntimeElementContainer(),
                areas = RuntimeStateCopy.Clone(match.areas) ?? new RuntimeAreaContainer(),
                slots = RuntimeStateCopy.Clone(match.slots) ?? new RuntimeSlotContainer(),
                topologies = RuntimeStateCopy.Clone(match.topologies) ?? new RuntimeTopologyContainer(),
                properties = RuntimeStateCopy.Clone(match.properties) ?? new ValueMap(),
                temps = RuntimeStateCopy.Clone(match.temps) ?? new MatchTempState(),
                idCounters = RuntimeStateCopy.Clone(match.idCounters) ?? new RuntimeIdCounters(),
                boxStock = RuntimeStateCopy.Clone(match.boxStock) ?? new BoxStockState(),
                random = RuntimeStateCopy.Clone(match.random) ?? new RuntimeRandomState(),
                interaction = RuntimeStateCopy.Clone(match.interaction) ?? new MatchInteractionState()
            };
        }

        public static void Restore(MatchState match, MatchStateSnapshot snapshot)
        {
            match.progression = RuntimeStateCopy.Clone(snapshot.progression) ?? new MatchProgressionState();
            match.execution = RuntimeStateCopy.Clone(snapshot.execution) ?? new MatchExecutionState();
            match.players = RuntimeStateCopy.Clone(snapshot.players) ?? new RuntimePlayerContainer();
            match.elements = RuntimeStateCopy.Clone(snapshot.elements) ?? new RuntimeElementContainer();
            match.areas = RuntimeStateCopy.Clone(snapshot.areas) ?? new RuntimeAreaContainer();
            match.slots = RuntimeStateCopy.Clone(snapshot.slots) ?? new RuntimeSlotContainer();
            match.topologies = RuntimeStateCopy.Clone(snapshot.topologies) ?? new RuntimeTopologyContainer();
            match.properties = RuntimeStateCopy.Clone(snapshot.properties) ?? new ValueMap();
            match.temps = RuntimeStateCopy.Clone(snapshot.temps) ?? new MatchTempState();
            match.idCounters = RuntimeStateCopy.Clone(snapshot.idCounters) ?? new RuntimeIdCounters();
            match.boxStock = RuntimeStateCopy.Clone(snapshot.boxStock) ?? new BoxStockState();
            match.random = RuntimeStateCopy.Clone(snapshot.random) ?? new RuntimeRandomState();
            match.interaction = RuntimeStateCopy.Clone(snapshot.interaction) ?? new MatchInteractionState();
        }
    }

    internal static class MatchHistoryTimeline
    {
        public static void TrackProgression(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasProgressionBefore)
                return;

            pending.hasProgressionBefore = true;
            pending.progressionBefore = RuntimeStateCopy.Clone(match.progression);
        }

        public static void TrackExecution(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasExecutionBefore)
                return;

            pending.hasExecutionBefore = true;
            pending.executionBefore = RuntimeStateCopy.Clone(match.execution);
        }

        public static void TrackInteraction(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasInteractionBefore)
                return;

            pending.hasInteractionBefore = true;
            pending.interactionBefore = RuntimeStateCopy.Clone(match.interaction);
        }

        public static void TrackMatchProperties(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasPropertiesBefore)
                return;

            pending.hasPropertiesBefore = true;
            pending.propertiesBefore = RuntimeStateCopy.Clone(match.properties);
        }

        public static void TrackTemps(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasTempsBefore)
                return;

            pending.hasTempsBefore = true;
            pending.tempsBefore = RuntimeStateCopy.Clone(match.temps);
        }

        public static void TrackIdCounters(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasIdCountersBefore)
                return;

            pending.hasIdCountersBefore = true;
            pending.idCountersBefore = RuntimeStateCopy.Clone(match.idCounters);
        }

        public static void TrackRandom(MatchState match)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.hasRandomBefore)
                return;

            pending.hasRandomBefore = true;
            pending.randomBefore = RuntimeStateCopy.Clone(match.random);
        }

        public static void TrackBoxStock(MatchState match, int definitionIndex)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.boxStockCountsBefore.ContainsKey(definitionIndex))
                return;

            pending.boxStockCountsBefore.Add(definitionIndex, match.GetBoxStockEntry(definitionIndex).availableCount);
        }

        public static void TrackPlayer(MatchState match, int playerId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.playerChanges.ContainsKey(playerId))
                return;

            pending.playerChanges.Add(playerId, new PendingRecordChange<RuntimePlayerRecord>
            {
                existedBefore = true,
                beforeIndex = match.GetPlayerIndex(playerId),
                before = RuntimeStateCopy.Clone(match.GetPlayer(playerId))
            });
        }

        public static void TrackPlayerAdded(MatchState match, int playerId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.playerChanges.ContainsKey(playerId))
                return;

            pending.playerChanges.Add(playerId, new PendingRecordChange<RuntimePlayerRecord>
            {
                existedBefore = false
            });
        }

        public static void TrackElement(MatchState match, int elementId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.elementChanges.ContainsKey(elementId))
                return;

            pending.elementChanges.Add(elementId, new PendingRecordChange<RuntimeElementRecord>
            {
                existedBefore = true,
                beforeIndex = match.GetElementIndex(elementId),
                before = RuntimeStateCopy.Clone(match.GetElement(elementId))
            });
        }

        public static void TrackElementAdded(MatchState match, int elementId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.elementChanges.ContainsKey(elementId))
                return;

            pending.elementChanges.Add(elementId, new PendingRecordChange<RuntimeElementRecord>
            {
                existedBefore = false
            });
        }

        public static void TrackArea(MatchState match, int areaId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.areaChanges.ContainsKey(areaId))
                return;

            pending.areaChanges.Add(areaId, new PendingRecordChange<RuntimeAreaRecord>
            {
                existedBefore = true,
                beforeIndex = match.GetAreaIndex(areaId),
                before = RuntimeStateCopy.Clone(match.GetArea(areaId))
            });
        }

        public static void TrackAreaAdded(MatchState match, int areaId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.areaChanges.ContainsKey(areaId))
                return;

            pending.areaChanges.Add(areaId, new PendingRecordChange<RuntimeAreaRecord>
            {
                existedBefore = false
            });
        }

        public static void TrackSlot(MatchState match, int slotId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.slotChanges.ContainsKey(slotId))
                return;

            pending.slotChanges.Add(slotId, new PendingRecordChange<RuntimeSlotRecord>
            {
                existedBefore = true,
                beforeIndex = match.GetSlotIndex(slotId),
                before = RuntimeStateCopy.Clone(match.GetSlot(slotId))
            });
        }

        public static void TrackSlotAdded(MatchState match, int slotId)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            if (pending.slotChanges.ContainsKey(slotId))
                return;

            pending.slotChanges.Add(slotId, new PendingRecordChange<RuntimeSlotRecord>
            {
                existedBefore = false
            });
        }

        public static void TrackTopology(MatchState match, int ownerElementId, string topologyKey)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            RuntimeTopologyHandle handle = new(ownerElementId, topologyKey);
            if (pending.topologyChanges.ContainsKey(handle))
                return;

            pending.topologyChanges.Add(handle, new PendingRecordChange<RuntimeTopologyRecord>
            {
                existedBefore = true,
                beforeIndex = match.GetTopologyIndex(ownerElementId, topologyKey),
                before = RuntimeStateCopy.Clone(match.GetRuntimeTopology(topologyKey, ownerElementId))
            });
        }

        public static void TrackTopologyAdded(MatchState match, int ownerElementId, string topologyKey)
        {
            if (!TryGetPending(match, out MatchPendingHistory pending))
                return;

            RuntimeTopologyHandle handle = new(ownerElementId, topologyKey);
            if (pending.topologyChanges.ContainsKey(handle))
                return;

            pending.topologyChanges.Add(handle, new PendingRecordChange<RuntimeTopologyRecord>
            {
                existedBefore = false
            });
        }

        public static void Capture(MatchState match, MatchCheckpointMetadata metadata)
        {
            MatchHistoryState history = match.history ??= new MatchHistoryState();
            history.pending ??= new MatchPendingHistory();

            if (!history.initialized)
            {
                history.initialized = true;
                history.currentIndex = 0;
                history.baselineMetadata = RuntimeStateCopy.Clone(metadata) ?? new MatchCheckpointMetadata();
                history.pending.Clear();
                return;
            }

            if (!history.pending.HasTrackedChanges())
                return;

            MatchStateChangeSet changes = BuildChangeSet(match, history.pending);
            history.pending.Clear();
            if (changes == null || !changes.HasAnyChanges())
                return;

            TrimFuture(history);

            MatchCheckpointRecord record = new()
            {
                id = history.nextCheckpointId++,
                metadata = RuntimeStateCopy.Clone(metadata) ?? new MatchCheckpointMetadata(),
                changes = changes
            };

            history.checkpoints.Add(record);
            history.currentIndex = history.checkpoints.Count;
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

            Apply(match, record.changes, false);
            history.currentIndex--;
            history.pending?.Clear();
            match.transcript.pendingEntries.Clear();
            return true;
        }

        public static bool Redo(MatchState match)
        {
            if (!CanRedo(match))
                return false;

            MatchHistoryState history = match.history;
            MatchCheckpointRecord record = history.checkpoints[history.currentIndex];

            Apply(match, record.changes, true);
            history.currentIndex++;
            history.pending?.Clear();
            match.transcript.pendingEntries.Clear();
            return true;
        }

        private static bool TryGetPending(MatchState match, out MatchPendingHistory pending)
        {
            pending = null;
            if (match == null || match.history == null)
                return false;

            MatchHistoryState history = match.history;
            if (!history.initialized || history.isApplyingHistory)
                return false;

            pending = history.pending ??= new MatchPendingHistory();
            return true;
        }

        private static MatchStateChangeSet BuildChangeSet(MatchState match, MatchPendingHistory pending)
        {
            MatchStateChangeSet changes = new();

            if (pending.hasProgressionBefore && !RuntimeStateComparer.AreEqual(pending.progressionBefore, match.progression))
            {
                changes.progressionChanged = true;
                changes.progressionBefore = pending.progressionBefore;
                changes.progressionAfter = RuntimeStateCopy.Clone(match.progression);
            }

            if (pending.hasExecutionBefore && !RuntimeStateComparer.AreEqual(pending.executionBefore, match.execution))
            {
                changes.executionChanged = true;
                changes.executionBefore = pending.executionBefore;
                changes.executionAfter = RuntimeStateCopy.Clone(match.execution);
            }

            if (pending.hasInteractionBefore && !RuntimeStateComparer.AreEqual(pending.interactionBefore, match.interaction))
            {
                changes.interactionChanged = true;
                changes.interactionBefore = pending.interactionBefore;
                changes.interactionAfter = RuntimeStateCopy.Clone(match.interaction);
            }

            if (pending.hasPropertiesBefore && !RuntimeStateComparer.AreEqual(pending.propertiesBefore, match.properties))
            {
                changes.propertiesChanged = true;
                changes.propertiesBefore = pending.propertiesBefore;
                changes.propertiesAfter = RuntimeStateCopy.Clone(match.properties);
            }

            if (pending.hasTempsBefore && !RuntimeStateComparer.AreEqual(pending.tempsBefore, match.temps))
            {
                changes.tempsChanged = true;
                changes.tempsBefore = pending.tempsBefore;
                changes.tempsAfter = RuntimeStateCopy.Clone(match.temps);
            }

            if (pending.hasIdCountersBefore && !RuntimeStateComparer.AreEqual(pending.idCountersBefore, match.idCounters))
            {
                changes.idCountersChanged = true;
                changes.idCountersBefore = pending.idCountersBefore;
                changes.idCountersAfter = RuntimeStateCopy.Clone(match.idCounters);
            }

            if (pending.hasRandomBefore && !RuntimeStateComparer.AreEqual(pending.randomBefore, match.random))
            {
                changes.randomChanged = true;
                changes.randomBefore = pending.randomBefore;
                changes.randomAfter = RuntimeStateCopy.Clone(match.random);
            }

            foreach (KeyValuePair<int, int> entry in pending.boxStockCountsBefore)
            {
                int afterCount = match.GetBoxStockEntry(entry.Key).availableCount;
                if (entry.Value == afterCount)
                    continue;

                changes.boxStockChanges ??= new List<BoxStockEntryChange>();
                changes.boxStockChanges.Add(new BoxStockEntryChange
                {
                    definitionIndex = entry.Key,
                    beforeCount = entry.Value,
                    afterCount = afterCount
                });
            }

            AppendPlayerChanges(match, pending, changes);
            AppendElementChanges(match, pending, changes);
            AppendAreaChanges(match, pending, changes);
            AppendSlotChanges(match, pending, changes);
            AppendTopologyChanges(match, pending, changes);

            return changes.HasAnyChanges() ? changes : null;
        }

        private static void AppendPlayerChanges(MatchState match, MatchPendingHistory pending, MatchStateChangeSet changes)
        {
            foreach (KeyValuePair<int, PendingRecordChange<RuntimePlayerRecord>> entry in pending.playerChanges)
            {
                int afterIndex = match.GetPlayerIndex(entry.Key);
                bool existedAfter = afterIndex != RuntimeIds.InvalidIndex;

                if (!entry.Value.existedBefore)
                {
                    if (!existedAfter)
                        continue;

                    changes.playerChanges ??= new List<RuntimePlayerRecordChange>();
                    changes.playerChanges.Add(new RuntimePlayerRecordChange
                    {
                        id = entry.Key,
                        existedBefore = false,
                        existedAfter = true,
                        beforeIndex = RuntimeIds.InvalidIndex,
                        afterIndex = afterIndex,
                        after = RuntimeStateCopy.Clone(match.players.items[afterIndex])
                    });
                    continue;
                }

                if (!existedAfter)
                {
                    changes.playerChanges ??= new List<RuntimePlayerRecordChange>();
                    changes.playerChanges.Add(new RuntimePlayerRecordChange
                    {
                        id = entry.Key,
                        existedBefore = true,
                        existedAfter = false,
                        beforeIndex = entry.Value.beforeIndex,
                        afterIndex = RuntimeIds.InvalidIndex,
                        before = entry.Value.before
                    });
                    continue;
                }

                RuntimePlayerRecord after = match.players.items[afterIndex];
                if (RuntimeStateComparer.AreEqual(entry.Value.before, after))
                    continue;

                changes.playerChanges ??= new List<RuntimePlayerRecordChange>();
                changes.playerChanges.Add(new RuntimePlayerRecordChange
                {
                    id = entry.Key,
                    existedBefore = true,
                    existedAfter = true,
                    beforeIndex = entry.Value.beforeIndex,
                    afterIndex = afterIndex,
                    before = entry.Value.before,
                    after = RuntimeStateCopy.Clone(after)
                });
            }
        }

        private static void AppendElementChanges(MatchState match, MatchPendingHistory pending, MatchStateChangeSet changes)
        {
            foreach (KeyValuePair<int, PendingRecordChange<RuntimeElementRecord>> entry in pending.elementChanges)
            {
                int afterIndex = match.GetElementIndex(entry.Key);
                bool existedAfter = afterIndex != RuntimeIds.InvalidIndex;

                if (!entry.Value.existedBefore)
                {
                    if (!existedAfter)
                        continue;

                    changes.elementChanges ??= new List<RuntimeElementRecordChange>();
                    changes.elementChanges.Add(new RuntimeElementRecordChange
                    {
                        id = entry.Key,
                        existedBefore = false,
                        existedAfter = true,
                        beforeIndex = RuntimeIds.InvalidIndex,
                        afterIndex = afterIndex,
                        after = RuntimeStateCopy.Clone(match.elements.items[afterIndex])
                    });
                    continue;
                }

                if (!existedAfter)
                {
                    changes.elementChanges ??= new List<RuntimeElementRecordChange>();
                    changes.elementChanges.Add(new RuntimeElementRecordChange
                    {
                        id = entry.Key,
                        existedBefore = true,
                        existedAfter = false,
                        beforeIndex = entry.Value.beforeIndex,
                        afterIndex = RuntimeIds.InvalidIndex,
                        before = entry.Value.before
                    });
                    continue;
                }

                RuntimeElementRecord after = match.elements.items[afterIndex];
                if (RuntimeStateComparer.AreEqual(entry.Value.before, after))
                    continue;

                changes.elementChanges ??= new List<RuntimeElementRecordChange>();
                changes.elementChanges.Add(new RuntimeElementRecordChange
                {
                    id = entry.Key,
                    existedBefore = true,
                    existedAfter = true,
                    beforeIndex = entry.Value.beforeIndex,
                    afterIndex = afterIndex,
                    before = entry.Value.before,
                    after = RuntimeStateCopy.Clone(after)
                });
            }
        }

        private static void AppendAreaChanges(MatchState match, MatchPendingHistory pending, MatchStateChangeSet changes)
        {
            foreach (KeyValuePair<int, PendingRecordChange<RuntimeAreaRecord>> entry in pending.areaChanges)
            {
                int afterIndex = match.GetAreaIndex(entry.Key);
                bool existedAfter = afterIndex != RuntimeIds.InvalidIndex;

                if (!entry.Value.existedBefore)
                {
                    if (!existedAfter)
                        continue;

                    changes.areaChanges ??= new List<RuntimeAreaRecordChange>();
                    changes.areaChanges.Add(new RuntimeAreaRecordChange
                    {
                        id = entry.Key,
                        existedBefore = false,
                        existedAfter = true,
                        beforeIndex = RuntimeIds.InvalidIndex,
                        afterIndex = afterIndex,
                        after = RuntimeStateCopy.Clone(match.areas.items[afterIndex])
                    });
                    continue;
                }

                if (!existedAfter)
                {
                    changes.areaChanges ??= new List<RuntimeAreaRecordChange>();
                    changes.areaChanges.Add(new RuntimeAreaRecordChange
                    {
                        id = entry.Key,
                        existedBefore = true,
                        existedAfter = false,
                        beforeIndex = entry.Value.beforeIndex,
                        afterIndex = RuntimeIds.InvalidIndex,
                        before = entry.Value.before
                    });
                    continue;
                }

                RuntimeAreaRecord after = match.areas.items[afterIndex];
                if (RuntimeStateComparer.AreEqual(entry.Value.before, after))
                    continue;

                changes.areaChanges ??= new List<RuntimeAreaRecordChange>();
                changes.areaChanges.Add(new RuntimeAreaRecordChange
                {
                    id = entry.Key,
                    existedBefore = true,
                    existedAfter = true,
                    beforeIndex = entry.Value.beforeIndex,
                    afterIndex = afterIndex,
                    before = entry.Value.before,
                    after = RuntimeStateCopy.Clone(after)
                });
            }
        }

        private static void AppendSlotChanges(MatchState match, MatchPendingHistory pending, MatchStateChangeSet changes)
        {
            foreach (KeyValuePair<int, PendingRecordChange<RuntimeSlotRecord>> entry in pending.slotChanges)
            {
                int afterIndex = match.GetSlotIndex(entry.Key);
                bool existedAfter = afterIndex != RuntimeIds.InvalidIndex;

                if (!entry.Value.existedBefore)
                {
                    if (!existedAfter)
                        continue;

                    changes.slotChanges ??= new List<RuntimeSlotRecordChange>();
                    changes.slotChanges.Add(new RuntimeSlotRecordChange
                    {
                        id = entry.Key,
                        existedBefore = false,
                        existedAfter = true,
                        beforeIndex = RuntimeIds.InvalidIndex,
                        afterIndex = afterIndex,
                        after = RuntimeStateCopy.Clone(match.slots.items[afterIndex])
                    });
                    continue;
                }

                if (!existedAfter)
                {
                    changes.slotChanges ??= new List<RuntimeSlotRecordChange>();
                    changes.slotChanges.Add(new RuntimeSlotRecordChange
                    {
                        id = entry.Key,
                        existedBefore = true,
                        existedAfter = false,
                        beforeIndex = entry.Value.beforeIndex,
                        afterIndex = RuntimeIds.InvalidIndex,
                        before = entry.Value.before
                    });
                    continue;
                }

                RuntimeSlotRecord after = match.slots.items[afterIndex];
                if (RuntimeStateComparer.AreEqual(entry.Value.before, after))
                    continue;

                changes.slotChanges ??= new List<RuntimeSlotRecordChange>();
                changes.slotChanges.Add(new RuntimeSlotRecordChange
                {
                    id = entry.Key,
                    existedBefore = true,
                    existedAfter = true,
                    beforeIndex = entry.Value.beforeIndex,
                    afterIndex = afterIndex,
                    before = entry.Value.before,
                    after = RuntimeStateCopy.Clone(after)
                });
            }
        }

        private static void AppendTopologyChanges(MatchState match, MatchPendingHistory pending, MatchStateChangeSet changes)
        {
            foreach (KeyValuePair<RuntimeTopologyHandle, PendingRecordChange<RuntimeTopologyRecord>> entry in pending.topologyChanges)
            {
                int afterIndex = match.GetTopologyIndex(entry.Key.ownerElementId, entry.Key.key);
                bool existedAfter = afterIndex != RuntimeIds.InvalidIndex;

                if (!entry.Value.existedBefore)
                {
                    if (!existedAfter)
                        continue;

                    changes.topologyChanges ??= new List<RuntimeTopologyRecordChange>();
                    changes.topologyChanges.Add(new RuntimeTopologyRecordChange
                    {
                        key = entry.Key.key,
                        ownerElementId = entry.Key.ownerElementId,
                        existedBefore = false,
                        existedAfter = true,
                        beforeIndex = RuntimeIds.InvalidIndex,
                        afterIndex = afterIndex,
                        after = RuntimeStateCopy.Clone(match.topologies.items[afterIndex])
                    });
                    continue;
                }

                if (!existedAfter)
                {
                    changes.topologyChanges ??= new List<RuntimeTopologyRecordChange>();
                    changes.topologyChanges.Add(new RuntimeTopologyRecordChange
                    {
                        key = entry.Key.key,
                        ownerElementId = entry.Key.ownerElementId,
                        existedBefore = true,
                        existedAfter = false,
                        beforeIndex = entry.Value.beforeIndex,
                        afterIndex = RuntimeIds.InvalidIndex,
                        before = entry.Value.before
                    });
                    continue;
                }

                RuntimeTopologyRecord after = match.topologies.items[afterIndex];
                if (RuntimeStateComparer.AreEqual(entry.Value.before, after))
                    continue;

                changes.topologyChanges ??= new List<RuntimeTopologyRecordChange>();
                changes.topologyChanges.Add(new RuntimeTopologyRecordChange
                {
                    key = entry.Key.key,
                    ownerElementId = entry.Key.ownerElementId,
                    existedBefore = true,
                    existedAfter = true,
                    beforeIndex = entry.Value.beforeIndex,
                    afterIndex = afterIndex,
                    before = entry.Value.before,
                    after = RuntimeStateCopy.Clone(after)
                });
            }
        }

        private static void Apply(MatchState match, MatchStateChangeSet changes, bool forward)
        {
            MatchHistoryState history = match.history;
            history.isApplyingHistory = true;

            try
            {
                ApplyPlayerChanges(match.players.items, changes.playerChanges, forward);
                ApplyElementChanges(match.elements.items, changes.elementChanges, forward);
                ApplyAreaChanges(match.areas.items, changes.areaChanges, forward);
                ApplySlotChanges(match.slots.items, changes.slotChanges, forward);
                ApplyTopologyChanges(match.topologies.items, changes.topologyChanges, forward);
                ApplyBoxStockChanges(match, changes.boxStockChanges, forward);

                if (changes.propertiesChanged)
                    match.properties = RuntimeStateCopy.Clone(forward ? changes.propertiesAfter : changes.propertiesBefore) ?? new ValueMap();

                if (changes.tempsChanged)
                    match.temps = RuntimeStateCopy.Clone(forward ? changes.tempsAfter : changes.tempsBefore) ?? new MatchTempState();

                if (changes.idCountersChanged)
                    match.idCounters = RuntimeStateCopy.Clone(forward ? changes.idCountersAfter : changes.idCountersBefore) ?? new RuntimeIdCounters();

                if (changes.randomChanged)
                    match.random = RuntimeStateCopy.Clone(forward ? changes.randomAfter : changes.randomBefore) ?? new RuntimeRandomState();

                if (changes.progressionChanged)
                    match.progression = RuntimeStateCopy.Clone(forward ? changes.progressionAfter : changes.progressionBefore) ?? new MatchProgressionState();

                if (changes.executionChanged)
                    match.execution = RuntimeStateCopy.Clone(forward ? changes.executionAfter : changes.executionBefore) ?? new MatchExecutionState();

                if (changes.interactionChanged)
                    match.interaction = RuntimeStateCopy.Clone(forward ? changes.interactionAfter : changes.interactionBefore) ?? new MatchInteractionState();
            }
            finally
            {
                history.isApplyingHistory = false;
            }
        }

        private static void ApplyBoxStockChanges(MatchState match, List<BoxStockEntryChange> changes, bool forward)
        {
            if (changes == null)
                return;

            for (int index = 0; index < changes.Count; index++)
            {
                BoxStockEntry stockEntry = match.GetBoxStockEntry(changes[index].definitionIndex);
                stockEntry.availableCount = forward ? changes[index].afterCount : changes[index].beforeCount;
            }
        }

        private static void ApplyPlayerChanges(List<RuntimePlayerRecord> items, List<RuntimePlayerRecordChange> changes, bool forward)
        {
            if (changes == null)
                return;

            RemovePlayerChanges(items, changes, forward);
            List<RuntimePlayerRecordChange> ordered = new(changes);
            ordered.Sort((left, right) => (forward ? left.afterIndex : left.beforeIndex).CompareTo(forward ? right.afterIndex : right.beforeIndex));

            for (int index = 0; index < ordered.Count; index++)
            {
                RuntimePlayerRecordChange change = ordered[index];
                bool targetExists = forward ? change.existedAfter : change.existedBefore;
                if (!targetExists)
                    continue;

                int currentIndex = FindPlayerIndex(items, change.id);
                RuntimePlayerRecord target = RuntimeStateCopy.Clone(forward ? change.after : change.before) ?? new RuntimePlayerRecord();
                if (currentIndex >= 0)
                {
                    items[currentIndex] = target;
                    continue;
                }

                int insertIndex = forward ? change.afterIndex : change.beforeIndex;
                if (insertIndex < 0 || insertIndex > items.Count)
                    insertIndex = items.Count;

                items.Insert(insertIndex, target);
            }
        }

        private static void ApplyElementChanges(List<RuntimeElementRecord> items, List<RuntimeElementRecordChange> changes, bool forward)
        {
            if (changes == null)
                return;

            RemoveElementChanges(items, changes, forward);
            List<RuntimeElementRecordChange> ordered = new(changes);
            ordered.Sort((left, right) => (forward ? left.afterIndex : left.beforeIndex).CompareTo(forward ? right.afterIndex : right.beforeIndex));

            for (int index = 0; index < ordered.Count; index++)
            {
                RuntimeElementRecordChange change = ordered[index];
                bool targetExists = forward ? change.existedAfter : change.existedBefore;
                if (!targetExists)
                    continue;

                int currentIndex = FindElementIndex(items, change.id);
                RuntimeElementRecord target = RuntimeStateCopy.Clone(forward ? change.after : change.before) ?? new RuntimeElementRecord();
                if (currentIndex >= 0)
                {
                    items[currentIndex] = target;
                    continue;
                }

                int insertIndex = forward ? change.afterIndex : change.beforeIndex;
                if (insertIndex < 0 || insertIndex > items.Count)
                    insertIndex = items.Count;

                items.Insert(insertIndex, target);
            }
        }

        private static void ApplyAreaChanges(List<RuntimeAreaRecord> items, List<RuntimeAreaRecordChange> changes, bool forward)
        {
            if (changes == null)
                return;

            RemoveAreaChanges(items, changes, forward);
            List<RuntimeAreaRecordChange> ordered = new(changes);
            ordered.Sort((left, right) => (forward ? left.afterIndex : left.beforeIndex).CompareTo(forward ? right.afterIndex : right.beforeIndex));

            for (int index = 0; index < ordered.Count; index++)
            {
                RuntimeAreaRecordChange change = ordered[index];
                bool targetExists = forward ? change.existedAfter : change.existedBefore;
                if (!targetExists)
                    continue;

                int currentIndex = FindAreaIndex(items, change.id);
                RuntimeAreaRecord target = RuntimeStateCopy.Clone(forward ? change.after : change.before) ?? new RuntimeAreaRecord();
                if (currentIndex >= 0)
                {
                    items[currentIndex] = target;
                    continue;
                }

                int insertIndex = forward ? change.afterIndex : change.beforeIndex;
                if (insertIndex < 0 || insertIndex > items.Count)
                    insertIndex = items.Count;

                items.Insert(insertIndex, target);
            }
        }

        private static void ApplySlotChanges(List<RuntimeSlotRecord> items, List<RuntimeSlotRecordChange> changes, bool forward)
        {
            if (changes == null)
                return;

            RemoveSlotChanges(items, changes, forward);
            List<RuntimeSlotRecordChange> ordered = new(changes);
            ordered.Sort((left, right) => (forward ? left.afterIndex : left.beforeIndex).CompareTo(forward ? right.afterIndex : right.beforeIndex));

            for (int index = 0; index < ordered.Count; index++)
            {
                RuntimeSlotRecordChange change = ordered[index];
                bool targetExists = forward ? change.existedAfter : change.existedBefore;
                if (!targetExists)
                    continue;

                int currentIndex = FindSlotIndex(items, change.id);
                RuntimeSlotRecord target = RuntimeStateCopy.Clone(forward ? change.after : change.before) ?? new RuntimeSlotRecord();
                if (currentIndex >= 0)
                {
                    items[currentIndex] = target;
                    continue;
                }

                int insertIndex = forward ? change.afterIndex : change.beforeIndex;
                if (insertIndex < 0 || insertIndex > items.Count)
                    insertIndex = items.Count;

                items.Insert(insertIndex, target);
            }
        }

        private static void ApplyTopologyChanges(List<RuntimeTopologyRecord> items, List<RuntimeTopologyRecordChange> changes, bool forward)
        {
            if (changes == null)
                return;

            RemoveTopologyChanges(items, changes, forward);
            List<RuntimeTopologyRecordChange> ordered = new(changes);
            ordered.Sort((left, right) => (forward ? left.afterIndex : left.beforeIndex).CompareTo(forward ? right.afterIndex : right.beforeIndex));

            for (int index = 0; index < ordered.Count; index++)
            {
                RuntimeTopologyRecordChange change = ordered[index];
                bool targetExists = forward ? change.existedAfter : change.existedBefore;
                if (!targetExists)
                    continue;

                int currentIndex = FindTopologyIndex(items, change.ownerElementId, change.key);
                RuntimeTopologyRecord target = RuntimeStateCopy.Clone(forward ? change.after : change.before) ?? new RuntimeTopologyRecord();
                if (currentIndex >= 0)
                {
                    items[currentIndex] = target;
                    continue;
                }

                int insertIndex = forward ? change.afterIndex : change.beforeIndex;
                if (insertIndex < 0 || insertIndex > items.Count)
                    insertIndex = items.Count;

                items.Insert(insertIndex, target);
            }
        }

        private static void RemovePlayerChanges(List<RuntimePlayerRecord> items, List<RuntimePlayerRecordChange> changes, bool forward)
        {
            for (int index = 0; index < changes.Count; index++)
            {
                bool targetExists = forward ? changes[index].existedAfter : changes[index].existedBefore;
                if (targetExists)
                    continue;

                int currentIndex = FindPlayerIndex(items, changes[index].id);
                if (currentIndex >= 0)
                    items.RemoveAt(currentIndex);
            }
        }

        private static void RemoveElementChanges(List<RuntimeElementRecord> items, List<RuntimeElementRecordChange> changes, bool forward)
        {
            for (int index = 0; index < changes.Count; index++)
            {
                bool targetExists = forward ? changes[index].existedAfter : changes[index].existedBefore;
                if (targetExists)
                    continue;

                int currentIndex = FindElementIndex(items, changes[index].id);
                if (currentIndex >= 0)
                    items.RemoveAt(currentIndex);
            }
        }

        private static void RemoveAreaChanges(List<RuntimeAreaRecord> items, List<RuntimeAreaRecordChange> changes, bool forward)
        {
            for (int index = 0; index < changes.Count; index++)
            {
                bool targetExists = forward ? changes[index].existedAfter : changes[index].existedBefore;
                if (targetExists)
                    continue;

                int currentIndex = FindAreaIndex(items, changes[index].id);
                if (currentIndex >= 0)
                    items.RemoveAt(currentIndex);
            }
        }

        private static void RemoveSlotChanges(List<RuntimeSlotRecord> items, List<RuntimeSlotRecordChange> changes, bool forward)
        {
            for (int index = 0; index < changes.Count; index++)
            {
                bool targetExists = forward ? changes[index].existedAfter : changes[index].existedBefore;
                if (targetExists)
                    continue;

                int currentIndex = FindSlotIndex(items, changes[index].id);
                if (currentIndex >= 0)
                    items.RemoveAt(currentIndex);
            }
        }

        private static void RemoveTopologyChanges(List<RuntimeTopologyRecord> items, List<RuntimeTopologyRecordChange> changes, bool forward)
        {
            for (int index = 0; index < changes.Count; index++)
            {
                bool targetExists = forward ? changes[index].existedAfter : changes[index].existedBefore;
                if (targetExists)
                    continue;

                int currentIndex = FindTopologyIndex(items, changes[index].ownerElementId, changes[index].key);
                if (currentIndex >= 0)
                    items.RemoveAt(currentIndex);
            }
        }

        private static int FindPlayerIndex(List<RuntimePlayerRecord> items, int id)
        {
            for (int index = 0; index < items.Count; index++)
            {
                if (items[index].id == id)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        private static int FindElementIndex(List<RuntimeElementRecord> items, int id)
        {
            for (int index = 0; index < items.Count; index++)
            {
                if (items[index].id == id)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        private static int FindAreaIndex(List<RuntimeAreaRecord> items, int id)
        {
            for (int index = 0; index < items.Count; index++)
            {
                if (items[index].id == id)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        private static int FindSlotIndex(List<RuntimeSlotRecord> items, int id)
        {
            for (int index = 0; index < items.Count; index++)
            {
                if (items[index].id == id)
                    return index;
            }

            return RuntimeIds.InvalidIndex;
        }

        private static int FindTopologyIndex(List<RuntimeTopologyRecord> items, int ownerElementId, string key)
        {
            for (int index = 0; index < items.Count; index++)
            {
                if (items[index].ownerElementId == ownerElementId &&
                    string.Equals(items[index].key, key, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return RuntimeIds.InvalidIndex;
        }

        private static void TrimFuture(MatchHistoryState history)
        {
            if (history.currentIndex >= history.checkpoints.Count)
                return;

            history.checkpoints.RemoveRange(history.currentIndex, history.checkpoints.Count - history.currentIndex);
        }
    }
}
