using System;
using System.Collections.Generic;

namespace TTA.Core
{
    internal static class RuntimeStateCopy
    {
        public static MatchCheckpointMetadata Clone(MatchCheckpointMetadata source)
        {
            if (source == null)
                return null;

            return new MatchCheckpointMetadata
            {
                interactionWindowId = source.interactionWindowId,
                mode = source.mode,
                actorPlayerId = source.actorPlayerId,
                phaseKey = source.phaseKey ?? string.Empty,
                sourceTrigger = source.sourceTrigger ?? string.Empty
            };
        }

        public static MatchProgressionState Clone(MatchProgressionState source)
        {
            if (source == null)
                return null;

            return new MatchProgressionState
            {
                rulesetKey = source.rulesetKey ?? string.Empty,
                currentPhaseKey = source.currentPhaseKey ?? string.Empty,
                currentPlayerId = source.currentPlayerId,
                ended = source.ended,
                winnerPlayerId = source.winnerPlayerId
            };
        }

        public static EventPayload Clone(EventPayload source)
        {
            if (source == null)
                return null;

            return new EventPayload
            {
                trigger = source.trigger ?? string.Empty,
                fields = Clone(source.fields),
                hasMovementData = source.hasMovementData,
                movementElementId = source.movementElementId,
                movementRequestedSteps = source.movementRequestedSteps,
                movementActualSteps = source.movementActualSteps,
                movementAreaId = source.movementAreaId,
                movementFinalAreaId = source.movementFinalAreaId,
                movementTopologyKey = source.movementTopologyKey ?? string.Empty,
                movementLinkName = source.movementLinkName ?? string.Empty
            };
        }

        public static MatchExecutionState Clone(MatchExecutionState source)
        {
            if (source == null)
                return null;

            MatchExecutionState copy = new()
            {
                mode = source.mode,
                queuedNextPhase = source.queuedNextPhase ?? string.Empty,
                resolvingPlayerId = source.resolvingPlayerId,
                hasCurrentEvent = source.hasCurrentEvent,
                currentEvent = Clone(source.currentEvent) ?? new EventPayload()
            };

            if (source.queuedEvents != null)
            {
                copy.queuedEvents = new List<EventPayload>(source.queuedEvents.Count);
                for (int index = 0; index < source.queuedEvents.Count; index++)
                    copy.queuedEvents.Add(Clone(source.queuedEvents[index]) ?? new EventPayload());
            }

            return copy;
        }

        public static InteractionWindow Clone(InteractionWindow source)
        {
            if (source == null)
                return null;

            InteractionWindow copy = new()
            {
                id = source.id,
                kind = source.kind,
                primaryPlayerId = source.primaryPlayerId,
                phaseKey = source.phaseKey ?? string.Empty,
                sourceTrigger = source.sourceTrigger ?? string.Empty,
                metadata = Clone(source.metadata) ?? new ValueMap()
            };

            if (source.eligiblePlayerIds != null)
                copy.eligiblePlayerIds = new List<int>(source.eligiblePlayerIds);

            return copy;
        }

        public static MatchInteractionState Clone(MatchInteractionState source)
        {
            if (source == null)
                return null;

            return new MatchInteractionState
            {
                currentWindow = Clone(source.currentWindow) ?? new InteractionWindow(),
                pendingActionPlayerId = source.pendingActionPlayerId
            };
        }

        public static RuntimeRandomState Clone(RuntimeRandomState source)
        {
            if (source == null)
                return null;

            return new RuntimeRandomState
            {
                seed = source.seed,
                state = source.state
            };
        }

        public static RuntimePlayerRecord Clone(RuntimePlayerRecord source)
        {
            if (source == null)
                return null;

            return new RuntimePlayerRecord
            {
                id = source.id,
                orderIndex = source.orderIndex,
                properties = Clone(source.properties) ?? new ValueMap(),
                temps = Clone(source.temps) ?? new ValueMap()
            };
        }

        public static RuntimeElementRecord Clone(RuntimeElementRecord source)
        {
            if (source == null)
                return null;

            return new RuntimeElementRecord
            {
                id = source.id,
                definitionIndex = source.definitionIndex,
                ownerPlayerId = source.ownerPlayerId,
                placementState = source.placementState,
                areaId = source.areaId,
                slotId = source.slotId,
                orderIndex = source.orderIndex,
                currentFaceIndex = source.currentFaceIndex,
                properties = Clone(source.properties) ?? new ValueMap(),
                temps = Clone(source.temps) ?? new ValueMap()
            };
        }

        public static RuntimeAreaRecord Clone(RuntimeAreaRecord source)
        {
            if (source == null)
                return null;

            RuntimeAreaRecord copy = new()
            {
                id = source.id,
                definitionIndex = source.definitionIndex,
                ownerElementId = source.ownerElementId,
                properties = Clone(source.properties) ?? new ValueMap(),
                temps = Clone(source.temps) ?? new ValueMap()
            };

            if (source.slotIds != null)
                copy.slotIds = new List<int>(source.slotIds);

            return copy;
        }

        public static RuntimeSlotRecord Clone(RuntimeSlotRecord source)
        {
            if (source == null)
                return null;

            RuntimeSlotRecord copy = new()
            {
                id = source.id,
                areaId = source.areaId,
                definitionIndex = source.definitionIndex
            };

            if (source.elementIds != null)
                copy.elementIds = new List<int>(source.elementIds);

            return copy;
        }

        public static RuntimeTopologyLinkRecord Clone(RuntimeTopologyLinkRecord source)
        {
            if (source == null)
                return null;

            return new RuntimeTopologyLinkRecord
            {
                fromAreaId = source.fromAreaId,
                toAreaId = source.toAreaId,
                name = source.name ?? string.Empty
            };
        }

        public static RuntimeTopologyRecord Clone(RuntimeTopologyRecord source)
        {
            if (source == null)
                return null;

            RuntimeTopologyRecord copy = new()
            {
                key = source.key ?? string.Empty,
                ownerElementId = source.ownerElementId
            };

            if (source.links != null)
            {
                copy.links = new List<RuntimeTopologyLinkRecord>(source.links.Count);
                for (int index = 0; index < source.links.Count; index++)
                    copy.links.Add(Clone(source.links[index]) ?? new RuntimeTopologyLinkRecord());
            }

            return copy;
        }

        public static RuntimePlayerContainer Clone(RuntimePlayerContainer source)
        {
            RuntimePlayerContainer copy = new();
            if (source?.items == null)
                return copy;

            copy.items = new List<RuntimePlayerRecord>(source.items.Count);
            for (int index = 0; index < source.items.Count; index++)
                copy.items.Add(Clone(source.items[index]) ?? new RuntimePlayerRecord());

            return copy;
        }

        public static RuntimeElementContainer Clone(RuntimeElementContainer source)
        {
            RuntimeElementContainer copy = new();
            if (source?.items == null)
                return copy;

            copy.items = new List<RuntimeElementRecord>(source.items.Count);
            for (int index = 0; index < source.items.Count; index++)
                copy.items.Add(Clone(source.items[index]) ?? new RuntimeElementRecord());

            return copy;
        }

        public static RuntimeAreaContainer Clone(RuntimeAreaContainer source)
        {
            RuntimeAreaContainer copy = new();
            if (source?.items == null)
                return copy;

            copy.items = new List<RuntimeAreaRecord>(source.items.Count);
            for (int index = 0; index < source.items.Count; index++)
                copy.items.Add(Clone(source.items[index]) ?? new RuntimeAreaRecord());

            return copy;
        }

        public static RuntimeSlotContainer Clone(RuntimeSlotContainer source)
        {
            RuntimeSlotContainer copy = new();
            if (source?.items == null)
                return copy;

            copy.items = new List<RuntimeSlotRecord>(source.items.Count);
            for (int index = 0; index < source.items.Count; index++)
                copy.items.Add(Clone(source.items[index]) ?? new RuntimeSlotRecord());

            return copy;
        }

        public static RuntimeTopologyContainer Clone(RuntimeTopologyContainer source)
        {
            RuntimeTopologyContainer copy = new();
            if (source?.items == null)
                return copy;

            copy.items = new List<RuntimeTopologyRecord>(source.items.Count);
            for (int index = 0; index < source.items.Count; index++)
                copy.items.Add(Clone(source.items[index]) ?? new RuntimeTopologyRecord());

            return copy;
        }

        public static ValueMap Clone(ValueMap source)
        {
            return source == null ? null : source.DeepCopy();
        }

        public static MatchTempState Clone(MatchTempState source)
        {
            if (source == null)
                return null;

            return new MatchTempState
            {
                match = Clone(source.match) ?? new ValueMap(),
                turn = Clone(source.turn) ?? new ValueMap(),
                setup = Clone(source.setup) ?? new ValueMap()
            };
        }

        public static RuntimeIdCounters Clone(RuntimeIdCounters source)
        {
            if (source == null)
                return null;

            return new RuntimeIdCounters
            {
                nextElementId = source.nextElementId,
                nextAreaId = source.nextAreaId,
                nextSlotId = source.nextSlotId,
                nextPlayerId = source.nextPlayerId,
                nextInteractionWindowId = source.nextInteractionWindowId
            };
        }

        public static BoxStockEntry Clone(BoxStockEntry source)
        {
            if (source == null)
                return null;

            return new BoxStockEntry
            {
                elementDefinitionIndex = source.elementDefinitionIndex,
                availableCount = source.availableCount
            };
        }

        public static BoxStockState Clone(BoxStockState source)
        {
            BoxStockState copy = new();
            if (source?.entries == null)
                return copy;

            copy.entries = new List<BoxStockEntry>(source.entries.Count);
            for (int index = 0; index < source.entries.Count; index++)
                copy.entries.Add(Clone(source.entries[index]) ?? new BoxStockEntry());

            return copy;
        }
    }

    internal static class RuntimeStateComparer
    {
        public static bool AreEqual(MatchProgressionState left, MatchProgressionState right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return SameString(left.rulesetKey, right.rulesetKey) &&
                SameString(left.currentPhaseKey, right.currentPhaseKey) &&
                left.currentPlayerId == right.currentPlayerId &&
                left.ended == right.ended &&
                left.winnerPlayerId == right.winnerPlayerId;
        }

        public static bool AreEqual(EventPayload left, EventPayload right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return SameString(left.trigger, right.trigger) &&
                AreEqual(left.fields, right.fields) &&
                left.hasMovementData == right.hasMovementData &&
                left.movementElementId == right.movementElementId &&
                left.movementRequestedSteps == right.movementRequestedSteps &&
                left.movementActualSteps == right.movementActualSteps &&
                left.movementAreaId == right.movementAreaId &&
                left.movementFinalAreaId == right.movementFinalAreaId &&
                SameString(left.movementTopologyKey, right.movementTopologyKey) &&
                SameString(left.movementLinkName, right.movementLinkName);
        }

        public static bool AreEqual(MatchExecutionState left, MatchExecutionState right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left.mode != right.mode ||
                !SameString(left.queuedNextPhase, right.queuedNextPhase) ||
                left.resolvingPlayerId != right.resolvingPlayerId ||
                left.hasCurrentEvent != right.hasCurrentEvent ||
                !AreEqual(left.currentEvent, right.currentEvent))
            {
                return false;
            }

            return AreEqual(left.queuedEvents, right.queuedEvents);
        }

        public static bool AreEqual(InteractionWindow left, InteractionWindow right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.id == right.id &&
                left.kind == right.kind &&
                left.primaryPlayerId == right.primaryPlayerId &&
                SameString(left.phaseKey, right.phaseKey) &&
                SameString(left.sourceTrigger, right.sourceTrigger) &&
                AreEqual(left.eligiblePlayerIds, right.eligiblePlayerIds) &&
                AreEqual(left.metadata, right.metadata);
        }

        public static bool AreEqual(MatchInteractionState left, MatchInteractionState right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.pendingActionPlayerId == right.pendingActionPlayerId &&
                AreEqual(left.currentWindow, right.currentWindow);
        }

        public static bool AreEqual(RuntimeRandomState left, RuntimeRandomState right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.seed == right.seed &&
                left.state == right.state;
        }

        public static bool AreEqual(RuntimePlayerRecord left, RuntimePlayerRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.id == right.id &&
                left.orderIndex == right.orderIndex &&
                AreEqual(left.properties, right.properties) &&
                AreEqual(left.temps, right.temps);
        }

        public static bool AreEqual(RuntimeElementRecord left, RuntimeElementRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.id == right.id &&
                left.definitionIndex == right.definitionIndex &&
                left.ownerPlayerId == right.ownerPlayerId &&
                left.placementState == right.placementState &&
                left.areaId == right.areaId &&
                left.slotId == right.slotId &&
                left.orderIndex == right.orderIndex &&
                left.currentFaceIndex == right.currentFaceIndex &&
                AreEqual(left.properties, right.properties) &&
                AreEqual(left.temps, right.temps);
        }

        public static bool AreEqual(RuntimeAreaRecord left, RuntimeAreaRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.id == right.id &&
                left.definitionIndex == right.definitionIndex &&
                left.ownerElementId == right.ownerElementId &&
                AreEqual(left.properties, right.properties) &&
                AreEqual(left.temps, right.temps) &&
                AreEqual(left.slotIds, right.slotIds);
        }

        public static bool AreEqual(RuntimeSlotRecord left, RuntimeSlotRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.id == right.id &&
                left.areaId == right.areaId &&
                left.definitionIndex == right.definitionIndex &&
                AreEqual(left.elementIds, right.elementIds);
        }

        public static bool AreEqual(RuntimeTopologyLinkRecord left, RuntimeTopologyLinkRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.fromAreaId == right.fromAreaId &&
                left.toAreaId == right.toAreaId &&
                SameString(left.name, right.name);
        }

        public static bool AreEqual(RuntimeTopologyRecord left, RuntimeTopologyRecord right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (!SameString(left.key, right.key) ||
                left.ownerElementId != right.ownerElementId)
            {
                return false;
            }

            if (left.links == null || right.links == null)
                return left.links == null && right.links == null;

            if (left.links.Count != right.links.Count)
                return false;

            for (int index = 0; index < left.links.Count; index++)
            {
                if (!AreEqual(left.links[index], right.links[index]))
                    return false;
            }

            return true;
        }

        public static bool AreEqual(ValueMap left, ValueMap right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left.entries.Count != right.entries.Count)
                return false;

            for (int index = 0; index < left.entries.Count; index++)
            {
                if (!SameString(left.entries[index].key, right.entries[index].key))
                    return false;

                if (!AreEqual(left.entries[index].value, right.entries[index].value))
                    return false;
            }

            return true;
        }

        public static bool AreEqual(MatchTempState left, MatchTempState right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return AreEqual(left.match, right.match) &&
                AreEqual(left.turn, right.turn) &&
                AreEqual(left.setup, right.setup);
        }

        public static bool AreEqual(RuntimeIdCounters left, RuntimeIdCounters right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            return left.nextElementId == right.nextElementId &&
                left.nextAreaId == right.nextAreaId &&
                left.nextSlotId == right.nextSlotId &&
                left.nextPlayerId == right.nextPlayerId &&
                left.nextInteractionWindowId == right.nextInteractionWindowId;
        }

        public static bool AreEqual(Value left, Value right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left.kind != right.kind ||
                left.intValue != right.intValue ||
                left.floatValue != right.floatValue ||
                left.boolValue != right.boolValue ||
                !SameString(left.stringValue, right.stringValue) ||
                !SameString(left.bindingPath, right.bindingPath) ||
                left.idValue != right.idValue ||
                left.collectionItemKind != right.collectionItemKind)
            {
                return false;
            }

            if (left.collectionItems == null || right.collectionItems == null)
                return left.collectionItems == null && right.collectionItems == null;

            if (left.collectionItems.Count != right.collectionItems.Count)
                return false;

            for (int index = 0; index < left.collectionItems.Count; index++)
            {
                if (!AreEqual(left.collectionItems[index], right.collectionItems[index]))
                    return false;
            }

            return true;
        }

        private static bool AreEqual(List<EventPayload> left, List<EventPayload> right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left.Count != right.Count)
                return false;

            for (int index = 0; index < left.Count; index++)
            {
                if (!AreEqual(left[index], right[index]))
                    return false;
            }

            return true;
        }

        private static bool AreEqual(List<int> left, List<int> right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left.Count != right.Count)
                return false;

            for (int index = 0; index < left.Count; index++)
            {
                if (left[index] != right[index])
                    return false;
            }

            return true;
        }

        private static bool SameString(string left, string right)
        {
            return string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.Ordinal);
        }
    }
}
