using System.Collections.Generic;
using System.Text;
using TTA.Core;

namespace TTA.Presenter
{

    public sealed class TextTranscriptPresenter
    {
        public List<string> CollectNewPublicBatches(MatchState match, ref int nextBatchIndex)
        {
            List<string> messages = new();
            if (match == null || match.transcript == null)
                return messages;

            int safeStart = nextBatchIndex < 0 ? 0 : nextBatchIndex;
            for (int index = safeStart; index < match.transcript.completedBatches.Count; index++)
            {
                TranscriptBatch batch = match.transcript.completedBatches[index];
                if (batch.observerPlayerId != RuntimeIds.InvalidId)
                    continue;

                messages.Add(FormatBatch(batch));
            }

            nextBatchIndex = match.transcript.completedBatches.Count;
            return messages;
        }

        private string FormatBatch(TranscriptBatch batch)
        {
            StringBuilder builder = new();
            builder.Append($"Transcript batch {batch.id} [{batch.stopReason}]");

            if (batch.interactionWindowId != RuntimeIds.InvalidId)
                builder.Append($" window {batch.interactionWindowId}");

            for (int index = 0; index < batch.entries.Count; index++)
            {
                var entry = batch.entries[index];
                if(entry.kind == TranscriptEntryKind.EventQueued)
                {
                    continue;
                }
                builder.AppendLine();
                builder.Append("- ");
                builder.Append(FormatEntry(entry));
            }

            return builder.ToString();
        }

        private string FormatEntry(TranscriptEntry entry)
        {
            return entry.kind switch
            {
                TranscriptEntryKind.ActionSubmitted => $"Player {FormatPlayer(entry.actorPlayerId)} chose action '{GetString(entry, "ActionKey")}' in phase '{GetString(entry, "Phase")}'.",
                TranscriptEntryKind.ReactionSubmitted => $"Player {FormatPlayer(entry.actorPlayerId)} chose reaction '{GetString(entry, "ReactionKey")}' in phase '{GetString(entry, "Phase")}'.",
                TranscriptEntryKind.PhaseChanged => $"Phase changed to '{GetString(entry, "Phase")}'.",
                TranscriptEntryKind.EventQueued => "",
                TranscriptEntryKind.EventResolved => FormatEvent(entry, false),
                TranscriptEntryKind.WaitOpened => FormatWait(entry),
                TranscriptEntryKind.ElementsTakenFromBox => $"Took {FormatElementList(entry)} from the box.",
                TranscriptEntryKind.ElementsPlaced => $"Placed {FormatElementList(entry)} in area '{GetString(entry, "AreaKey")}'.",
                TranscriptEntryKind.ElementsUnplaced => $"Unplaced {FormatElementList(entry)}.",
                TranscriptEntryKind.ElementsReturnedToBox => $"Returned {FormatElementList(entry)} to the box.",
                TranscriptEntryKind.ElementMoved => $"Moved {FormatPrimaryElement(entry)} from '{GetString(entry, "FromAreaKey")}' to '{GetString(entry, "ToAreaKey")}' (requested {GetInt(entry, "RequestedSteps")}, actual {GetInt(entry, "ActualSteps")}).",
                TranscriptEntryKind.FaceChanged => $"Changed {FormatElementList(entry)} face to '{GetString(entry, "FaceId")}'.",
                TranscriptEntryKind.RollResolved => $"Rolled {FormatElementList(entry)} for total {GetInt(entry, "Total")} [{FormatIntCollection(entry.fields.GetOrDefault("RolledValues"))}].",
                TranscriptEntryKind.TurnAdvanced => $"Turn advanced to player {FormatPlayer(GetRuntimeId(entry, "CurrentPlayerId"))}.",
                TranscriptEntryKind.MatchEnded => $"Match ended. Winner player id: {GetRuntimeId(entry, "WinnerPlayerId")}.",
                _ => string.IsNullOrWhiteSpace(entry.code) ? entry.kind.ToString() : $"{entry.kind}: {entry.code}"
            };
        }

        private string FormatEvent(TranscriptEntry entry, bool queued)
        {
            string trigger = GetString(entry, "Trigger");
            string prefix = queued ? "Queued" : "Resolved";

            return trigger switch
            {
                "OnPhaseStarted" => $"{prefix} phase start for '{GetString(entry, "Phase")}'.",
                "OnRolled" => $"{prefix} roll for {FormatElementList(entry)}: total {GetInt(entry, "Total")} [{FormatIntCollection(GetCollectionValue(entry, "Results", "RolledValues"))}].",
                "OnAreaPassed" => $"{FormatPrimaryElement(entry)} passed through '{GetString(entry, "AreaKey", "Area")}'.",
                "OnAreaLanded" => $"{FormatPrimaryElement(entry)} landed on '{GetString(entry, "AreaKey", "Area")}'.",
                "OnMovementCompleted" => $"{FormatPrimaryElement(entry)} stopped on '{GetString(entry, "FinalAreaKey", "AreaKey", "Area")}' after {GetInt(entry, "ActualSteps")} step(s).",
                "OnFaceChanged" => $"{prefix} face change for {FormatElementList(entry)} to '{GetString(entry, "FaceId")}'.",
                _ => $"{prefix} event '{trigger}'."
            };
        }

        private string FormatWait(TranscriptEntry entry)
        {
            string kind = GetString(entry, "Kind");
            int windowId = GetInt(entry, "WindowId");
            string phase = GetString(entry, "Phase");

            if (kind == "Reaction")
                return $"Opened reaction window {windowId} in phase '{phase}' for players {FormatPlayerCollection(entry.fields.GetOrDefault("EligiblePlayers"))}.";

            int primaryPlayerId = GetRuntimeId(entry, "PrimaryPlayerId");
            return $"Waiting for player {FormatPlayer(primaryPlayerId)} in phase '{phase}' on window {windowId}.";
        }

        private string FormatElementList(TranscriptEntry entry)
        {
            Value ids = entry.fields.GetOrDefault("ElementIds");
            Value keys = entry.fields.GetOrDefault("ElementKeys");

            if (ids == null || ids.kind != ValueKind.Collection || ids.collectionItems.Count == 0)
            {
                string singleKey = GetString(entry, "ElementKey");
                int singleId = GetRuntimeId(entry, "ElementId");
                return !string.IsNullOrWhiteSpace(singleKey)
                    ? $"{singleKey}#{singleId}"
                    : $"element {singleId}";
            }

            List<string> parts = new();
            for (int index = 0; index < ids.collectionItems.Count; index++)
            {
                int id = ids.collectionItems[index].idValue;
                string key = keys != null &&
                    keys.kind == ValueKind.Collection &&
                    index < keys.collectionItems.Count &&
                    keys.collectionItems[index].kind == ValueKind.String
                    ? keys.collectionItems[index].stringValue ?? string.Empty
                    : "element";

                parts.Add($"{key}#{id}");
            }

            return string.Join(", ", parts);
        }

        private string FormatPrimaryElement(TranscriptEntry entry)
        {
            string key = GetString(entry, "ElementKey");
            int id = GetRuntimeId(entry, "ElementId");
            if (!string.IsNullOrWhiteSpace(key) && id != RuntimeIds.InvalidId)
                return $"{key}#{id}";

            return FormatElementList(entry);
        }

        private string FormatIntCollection(Value value)
        {
            if (value == null || value.kind != ValueKind.Collection || value.collectionItems.Count == 0)
                return string.Empty;

            List<string> parts = new();
            for (int index = 0; index < value.collectionItems.Count; index++)
            {
                Value item = value.collectionItems[index];
                if (item.kind == ValueKind.Int)
                    parts.Add(item.intValue.ToString());
            }

            return string.Join(", ", parts);
        }

        private string FormatPlayerCollection(Value value)
        {
            if (value == null || value.kind != ValueKind.Collection || value.collectionItems.Count == 0)
                return "?";

            List<string> parts = new();
            for (int index = 0; index < value.collectionItems.Count; index++)
            {
                if (value.collectionItems[index].kind == ValueKind.PlayerId)
                    parts.Add(FormatPlayer(value.collectionItems[index].idValue));
            }

            return string.Join(", ", parts);
        }

        private string GetString(TranscriptEntry entry, string key)
        {
            Value value = entry.fields.GetOrDefault(key);
            return value != null && value.kind == ValueKind.String
                ? value.stringValue ?? string.Empty
                : string.Empty;
        }

        private string GetString(TranscriptEntry entry, string primaryKey, string fallbackKey)
        {
            string value = GetString(entry, primaryKey);
            return string.IsNullOrWhiteSpace(value) ? GetString(entry, fallbackKey) : value;
        }

        private string GetString(TranscriptEntry entry, string primaryKey, string fallbackKey, string finalKey)
        {
            string value = GetString(entry, primaryKey, fallbackKey);
            return string.IsNullOrWhiteSpace(value) ? GetString(entry, finalKey) : value;
        }

        private int GetInt(TranscriptEntry entry, string key)
        {
            Value value = entry.fields.GetOrDefault(key);
            return value != null && value.kind == ValueKind.Int
                ? value.intValue
                : 0;
        }

        private int GetRuntimeId(TranscriptEntry entry, string key)
        {
            Value value = entry.fields.GetOrDefault(key);
            return value != null && value.IsRuntimeId
                ? value.idValue
                : RuntimeIds.InvalidId;
        }

        private Value GetCollectionValue(TranscriptEntry entry, string primaryKey, string fallbackKey)
        {
            Value value = entry.fields.GetOrDefault(primaryKey);
            if (value != null && value.kind == ValueKind.Collection)
                return value;

            value = entry.fields.GetOrDefault(fallbackKey);
            return value != null && value.kind == ValueKind.Collection
                ? value
                : Value.Null();
        }

        private string FormatPlayer(int playerId)
        {
            return playerId == RuntimeIds.InvalidId ? "?" : playerId.ToString();
        }
    }

}