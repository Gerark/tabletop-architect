using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed partial class MatchRuntimeController
    {
        private void BeginTranscriptBatch(MatchState match)
        {
            match.transcript.pendingEntries.Clear();
        }

        private void RecordActionSubmitted(MatchState match, string actionKey, int actorPlayerId, int windowId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ActionSubmitted, actionKey, actorPlayerId);
            entry.fields.Set("ActionKey", Value.FromString(actionKey));
            entry.fields.Set("WindowId", Value.FromInt(windowId));
            entry.fields.Set("Phase", Value.FromString(match.progression.currentPhaseKey));
        }

        private void RecordReactionSubmitted(MatchState match, string reactionKey, int actorPlayerId, int windowId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ReactionSubmitted, reactionKey, actorPlayerId);
            entry.fields.Set("ReactionKey", Value.FromString(reactionKey));
            entry.fields.Set("WindowId", Value.FromInt(windowId));
            entry.fields.Set("Phase", Value.FromString(match.progression.currentPhaseKey));
        }

        private void RecordPhaseChanged(MatchState match, string phaseKey)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.PhaseChanged, phaseKey);
            entry.fields.Set("Phase", Value.FromString(phaseKey));
        }

        private void RecordEventQueued(MatchState match, EventPayload payload)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.EventQueued, payload.trigger);
            entry.fields.Set("Trigger", Value.FromString(payload.trigger));
            CopyPayloadFields(match, entry, payload);
        }

        private void RecordEventResolved(MatchState match, EventPayload payload)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.EventResolved, payload.trigger, match.execution.resolvingPlayerId);
            entry.fields.Set("Trigger", Value.FromString(payload.trigger));
            CopyPayloadFields(match, entry, payload);
        }

        private void RecordWaitOpened(MatchState match, InteractionWindow window, TranscriptStopReason stopReason)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.WaitOpened, window.kind.ToString(), window.primaryPlayerId);
            entry.fields.Set("WindowId", Value.FromInt(window.id));
            entry.fields.Set("Kind", Value.FromString(window.kind.ToString()));
            entry.fields.Set("Phase", Value.FromString(window.phaseKey));
            entry.fields.Set("StopReason", Value.FromString(stopReason.ToString()));

            if (window.primaryPlayerId != RuntimeIds.InvalidId)
                entry.fields.Set("PrimaryPlayerId", Value.FromPlayerId(window.primaryPlayerId));

            if (!string.IsNullOrWhiteSpace(window.sourceTrigger))
                entry.fields.Set("Trigger", Value.FromString(window.sourceTrigger));

            if (window.eligiblePlayerIds.Count > 0)
                entry.fields.Set("EligiblePlayers", CreatePlayerCollection(window.eligiblePlayerIds));
        }

        private void RecordElementsTakenFromBox(MatchState match, List<int> elementIds, int ownerPlayerId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ElementsTakenFromBox, "TakeFromBox", ownerPlayerId);
            SetElementFields(match, entry, elementIds);
            if (ownerPlayerId != RuntimeIds.InvalidId)
                entry.fields.Set("OwnerPlayerId", Value.FromPlayerId(ownerPlayerId));
        }

        private void RecordElementsPlaced(MatchState match, List<int> elementIds, int areaId, int slotId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ElementsPlaced, "PlaceElement", match.execution.resolvingPlayerId);
            SetElementFields(match, entry, elementIds);
            entry.fields.Set("AreaId", Value.FromAreaId(areaId));
            entry.fields.Set("AreaKey", Value.FromString(GetRuntimeAreaKey(match, areaId)));
            entry.fields.Set("SlotId", Value.FromSlotId(slotId));
        }

        private void RecordElementsUnplaced(MatchState match, List<int> elementIds)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ElementsUnplaced, "UnplaceElement");
            SetElementFields(match, entry, elementIds);
        }

        private void RecordElementsReturnedToBox(MatchState match, List<int> elementIds)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ElementsReturnedToBox, "ReturnToBox");
            SetElementFields(match, entry, elementIds);
        }

        private void RecordElementMoved(MatchState match, int elementId, int fromAreaId, int toAreaId, int requestedSteps, int actualSteps)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.ElementMoved, "Move", match.execution.resolvingPlayerId);
            entry.fields.Set("ElementId", Value.FromElementId(elementId));
            entry.fields.Set("ElementKey", Value.FromString(GetElementDefinition(match.GetElement(elementId)).key));
            entry.fields.Set("FromAreaId", Value.FromAreaId(fromAreaId));
            entry.fields.Set("FromAreaKey", Value.FromString(GetRuntimeAreaKey(match, fromAreaId)));
            entry.fields.Set("ToAreaId", Value.FromAreaId(toAreaId));
            entry.fields.Set("ToAreaKey", Value.FromString(GetRuntimeAreaKey(match, toAreaId)));
            entry.fields.Set("RequestedSteps", Value.FromInt(requestedSteps));
            entry.fields.Set("ActualSteps", Value.FromInt(actualSteps));
        }

        private void RecordFaceChanged(MatchState match, List<int> elementIds, string faceId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.FaceChanged, "FaceChanged");
            SetElementFields(match, entry, elementIds);

            int hiddenOwnerPlayerId = GetPrivateOwnerPlayerId(match, elementIds);
            if (hiddenOwnerPlayerId == RuntimeIds.InvalidId)
            {
                entry.fields.Set("FaceId", Value.FromString(faceId));
                return;
            }

            entry.fields.Set("FaceId", Value.FromString("Hidden"));
            AddPrivateField(entry, "FaceId", hiddenOwnerPlayerId, Value.FromString(faceId), Value.FromString("Hidden"));
        }

        private void RecordRollResolved(MatchState match, List<int> elementIds, int total, List<int> rolledValues)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.RollResolved, "Roll", match.execution.resolvingPlayerId);
            SetElementFields(match, entry, elementIds);
            entry.fields.Set("Total", Value.FromInt(total));
            entry.fields.Set("RolledValues", CreateIntCollection(rolledValues));
        }

        private void RecordTurnAdvanced(MatchState match, int currentPlayerId)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.TurnAdvanced, "AdvanceTurn");
            entry.fields.Set("CurrentPlayerId", Value.FromPlayerId(currentPlayerId));
        }

        private void RecordMatchEnded(MatchState match)
        {
            TranscriptEntry entry = CreateTranscriptEntry(match, TranscriptEntryKind.MatchEnded, "Ended");
            if (match.progression.winnerPlayerId != RuntimeIds.InvalidId)
                entry.fields.Set("WinnerPlayerId", Value.FromPlayerId(match.progression.winnerPlayerId));
        }

        private void FlushTranscriptBatch(MatchState match, TranscriptStopReason stopReason, InteractionWindow window)
        {
            if (match.transcript.pendingEntries.Count == 0)
                return;

            match.transcript.completedBatches.Add(MaterializeTranscriptBatch(match, stopReason, window, RuntimeIds.InvalidId));
            for (int index = 0; index < match.players.items.Count; index++)
            {
                int observerPlayerId = match.players.items[index].id;
                match.transcript.completedBatches.Add(MaterializeTranscriptBatch(match, stopReason, window, observerPlayerId));
            }

            match.transcript.pendingEntries.Clear();
        }

        private TranscriptBatch MaterializeTranscriptBatch(MatchState match, TranscriptStopReason stopReason, InteractionWindow window, int observerPlayerId)
        {
            TranscriptBatch batch = new()
            {
                id = match.transcript.nextBatchId++,
                observerPlayerId = observerPlayerId,
                stopReason = stopReason,
                interactionWindowId = window?.id ?? RuntimeIds.InvalidId
            };

            batch.metadata.Set("Phase", Value.FromString(match.progression.currentPhaseKey));
            batch.metadata.Set("Mode", Value.FromString(match.execution.mode.ToString()));

            if (window != null)
            {
                batch.metadata.Set("WindowKind", Value.FromString(window.kind.ToString()));
                if (window.primaryPlayerId != RuntimeIds.InvalidId)
                    batch.metadata.Set("PrimaryPlayerId", Value.FromPlayerId(window.primaryPlayerId));
                if (!string.IsNullOrWhiteSpace(window.sourceTrigger))
                    batch.metadata.Set("Trigger", Value.FromString(window.sourceTrigger));
            }

            for (int index = 0; index < match.transcript.pendingEntries.Count; index++)
                batch.entries.Add(MaterializeTranscriptEntry(match.transcript.pendingEntries[index], observerPlayerId));

            return batch;
        }

        private TranscriptEntry MaterializeTranscriptEntry(TranscriptEntry source, int observerPlayerId)
        {
            TranscriptEntry entry = new()
            {
                kind = source.kind,
                code = source.code,
                actorPlayerId = source.actorPlayerId,
                fields = source.fields.DeepCopy()
            };

            for (int index = 0; index < source.privateFields.Count; index++)
            {
                TranscriptPrivateField privateField = source.privateFields[index];
                Value value = observerPlayerId == privateField.visibleToPlayerId
                    ? privateField.visibleValue
                    : privateField.hiddenValue;

                if (value != null && !value.IsNull)
                    entry.fields.Set(privateField.key, value);
                else if (!entry.fields.Contains(privateField.key))
                    entry.fields.Set(privateField.key, Value.Null());
            }

            return entry;
        }

        private TranscriptEntry CreateTranscriptEntry(MatchState match, TranscriptEntryKind kind, string code, int actorPlayerId = RuntimeIds.InvalidId)
        {
            return CreateTranscriptEntry(kind, code, actorPlayerId, match.transcript.pendingEntries);
        }

        private TranscriptEntry CreateTranscriptEntry(TranscriptEntryKind kind, string code, int actorPlayerId, List<TranscriptEntry> sink)
        {
            TranscriptEntry entry = new()
            {
                kind = kind,
                code = code ?? string.Empty,
                actorPlayerId = actorPlayerId
            };

            sink.Add(entry);
            return entry;
        }

        private void AddPrivateField(TranscriptEntry entry, string key, int visibleToPlayerId, Value visibleValue, Value hiddenValue)
        {
            entry.privateFields.Add(new TranscriptPrivateField
            {
                key = key ?? string.Empty,
                visibleToPlayerId = visibleToPlayerId,
                visibleValue = visibleValue ?? Value.Null(),
                hiddenValue = hiddenValue ?? Value.Null()
            });
        }

        private Value CreatePlayerCollection(List<int> playerIds)
        {
            List<Value> values = new(playerIds.Count);
            for (int index = 0; index < playerIds.Count; index++)
                values.Add(Value.FromPlayerId(playerIds[index]));

            return Value.FromCollection(values);
        }

        private Value CreateIntCollection(List<int> values)
        {
            List<Value> result = new(values.Count);
            for (int index = 0; index < values.Count; index++)
                result.Add(Value.FromInt(values[index]));

            return Value.FromCollection(result);
        }

        private int GetPrivateOwnerPlayerId(MatchState match, List<int> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0)
                return RuntimeIds.InvalidId;

            int ownerPlayerId = RuntimeIds.InvalidId;
            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.ownerPlayerId == RuntimeIds.InvalidId || element.placementState != PlacementState.Unplaced)
                    return RuntimeIds.InvalidId;

                if (ownerPlayerId == RuntimeIds.InvalidId)
                {
                    ownerPlayerId = element.ownerPlayerId;
                    continue;
                }

                if (ownerPlayerId != element.ownerPlayerId)
                    return RuntimeIds.InvalidId;
            }

            return ownerPlayerId;
        }

        private void CopyPayloadFields(MatchState match, TranscriptEntry entry, EventPayload payload)
        {
            if (payload == null || payload.fields == null)
                return;

            entry.fields = payload.fields.DeepCopy();
            entry.fields.Set("Trigger", Value.FromString(payload.trigger));
            int hiddenOwnerPlayerId = RuntimeIds.InvalidId;

            if (payload.fields.TryGetValue("ElementId", out Value elementId) && elementId.kind == ValueKind.ElementId)
            {
                entry.fields.Set("ElementKey", Value.FromString(GetElementDefinition(match.GetElement(elementId.idValue)).key));
                hiddenOwnerPlayerId = GetPrivateOwnerPlayerId(match, new List<int> { elementId.idValue });
            }

            if (payload.fields.TryGetValue("ElementIds", out Value elementIds) &&
                elementIds.kind == ValueKind.Collection &&
                elementIds.collectionItemKind == ValueKind.ElementId)
            {
                List<int> ids = new();
                for (int index = 0; index < elementIds.collectionItems.Count; index++)
                    ids.Add(elementIds.collectionItems[index].idValue);

                entry.fields.Set("ElementKeys", CreateElementKeyCollection(match, ids));
                hiddenOwnerPlayerId = GetPrivateOwnerPlayerId(match, ids);
            }

            if (hiddenOwnerPlayerId != RuntimeIds.InvalidId &&
                payload.fields.TryGetValue("FaceId", out Value faceId) &&
                faceId.kind == ValueKind.String)
            {
                entry.fields.Set("FaceId", Value.FromString("Hidden"));
                AddPrivateField(entry, "FaceId", hiddenOwnerPlayerId, faceId, Value.FromString("Hidden"));
            }
        }

        private void SetElementFields(MatchState match, TranscriptEntry entry, List<int> elementIds)
        {
            entry.fields.Set("ElementIds", CreateElementCollection(elementIds));
            entry.fields.Set("ElementKeys", CreateElementKeyCollection(match, elementIds));

            if (elementIds.Count == 1)
                entry.fields.Set("ElementKey", Value.FromString(GetElementDefinition(match.GetElement(elementIds[0])).key));
        }

        private Value CreateElementKeyCollection(MatchState match, List<int> elementIds)
        {
            List<Value> values = new(elementIds.Count);
            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                values.Add(Value.FromString(GetElementDefinition(element).key));
            }

            return Value.FromCollection(values);
        }
    }
}
