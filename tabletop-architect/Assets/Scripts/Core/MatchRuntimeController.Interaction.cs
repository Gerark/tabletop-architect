using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed partial class MatchRuntimeController
    {
        public InteractionWindow GetCurrentInteractionWindow(MatchState match)
        {
            return match?.interaction?.currentWindow ?? new InteractionWindow();
        }

        public IReadOnlyList<ReactionDefinition> GetAvailableReactions(MatchState match, int playerId)
        {
            List<ReactionDefinition> availableReactions = new();

            if (match == null || match.progression.ended || match.execution.mode != MatchExecutionMode.WaitingForReaction)
                return availableReactions;

            InteractionWindow window = GetCurrentInteractionWindow(match);
            if (window.kind != InteractionWindowKind.Reaction)
                return availableReactions;

            if (window.eligiblePlayerIds.Count > 0 && !window.eligiblePlayerIds.Contains(playerId))
                return availableReactions;

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            return GetAvailableReactionsForPlayer(match, phase, match.execution.currentEvent, playerId);
        }

        public void SubmitAction(MatchState match, int windowId, string actionKey)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (match.progression.ended)
                throw new InvalidOperationException("The match has already ended.");

            if (string.IsNullOrWhiteSpace(actionKey))
                throw new InvalidOperationException("Action key is required.");

            InteractionWindow window = GetCurrentInteractionWindow(match);
            EnsureWindow(window, InteractionWindowKind.PlayerAction, windowId);

            int actorPlayerId = window.primaryPlayerId != RuntimeIds.InvalidId
                ? window.primaryPlayerId
                : match.progression.currentPlayerId;

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            ExecutionContext context = new()
            {
                actingPlayerId = actorPlayerId
            };
            RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);

            if (!DoesPhaseAllowPlayer(match, phase, resolver, actorPlayerId))
                throw new InvalidOperationException("The submitted player action is not legal in the active interaction window.");

            PlayerActionDefinition selectedAction = null;
            for (int index = 0; index < phase.availableActions.Length; index++)
            {
                PlayerActionDefinition action = phase.availableActions[index];
                if (!string.Equals(action.key, actionKey, StringComparison.Ordinal))
                    continue;

                if (!ConditionEvaluator.Evaluate(action.when, resolver))
                    continue;

                selectedAction = action;
                break;
            }

            if (selectedAction == null)
                throw new InvalidOperationException($"Action '{actionKey}' is not available for interaction window {windowId}.");

            BeginTranscriptBatch(match);
            RecordActionSubmitted(match, actionKey, actorPlayerId, windowId);

            ClearCurrentWindow(match);
            match.execution.mode = MatchExecutionMode.Resolving;
            match.execution.resolvingPlayerId = actorPlayerId;

            ExecuteOperations(match, selectedAction.operations, context);
            FinishResolution(match);
        }

        public void SubmitReaction(MatchState match, int windowId, int playerId, string reactionKey)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (match.progression.ended)
                throw new InvalidOperationException("The match has already ended.");

            if (string.IsNullOrWhiteSpace(reactionKey))
                throw new InvalidOperationException("Reaction key is required.");

            InteractionWindow window = GetCurrentInteractionWindow(match);
            EnsureWindow(window, InteractionWindowKind.Reaction, windowId);

            if (window.eligiblePlayerIds.Count > 0 && !window.eligiblePlayerIds.Contains(playerId))
                throw new InvalidOperationException($"Player {playerId} is not eligible for interaction window {windowId}.");

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            List<ReactionDefinition> availableReactions = GetAvailableReactionsForPlayer(match, phase, match.execution.currentEvent, playerId);

            ReactionDefinition selectedReaction = null;
            for (int index = 0; index < availableReactions.Count; index++)
            {
                if (string.Equals(availableReactions[index].key, reactionKey, StringComparison.Ordinal))
                {
                    selectedReaction = availableReactions[index];
                    break;
                }
            }

            if (selectedReaction == null)
                throw new InvalidOperationException($"Reaction '{reactionKey}' is not available for interaction window {windowId}.");

            BeginTranscriptBatch(match);
            RecordReactionSubmitted(match, reactionKey, playerId, windowId);

            EventPayload reactionSource = RuntimeDataClone.Clone(match.execution.currentEvent) ?? new EventPayload();
            ClearCurrentWindow(match);
            match.execution.mode = MatchExecutionMode.Resolving;
            match.execution.resolvingPlayerId = playerId;

            ExecutionContext context = new()
            {
                actingPlayerId = playerId,
                eventPayload = reactionSource,
                eventTemps = new ValueMap()
            };

            ExecuteOperations(match, selectedReaction.operations, context);

            if (!string.IsNullOrWhiteSpace(selectedReaction.nextPhase))
            {
                match.execution.queuedNextPhase = selectedReaction.nextPhase;
                match.interaction.pendingActionPlayerId = playerId;
            }

            match.execution.hasCurrentEvent = false;
            match.execution.currentEvent = new EventPayload();

            FinishResolution(match);
        }

        public bool CanUndo(MatchState match)
        {
            return MatchHistoryTimeline.CanUndo(match);
        }

        public bool CanRedo(MatchState match)
        {
            return MatchHistoryTimeline.CanRedo(match);
        }

        public bool Undo(MatchState match)
        {
            return MatchHistoryTimeline.Undo(match);
        }

        public bool Redo(MatchState match)
        {
            return MatchHistoryTimeline.Redo(match);
        }

        public void ResetHistory(MatchState match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            match.history = new MatchHistoryState();
            CaptureCurrentCheckpoint(match);
        }

        private List<PlayerActionDefinition> GetAvailableActionsForPlayer(MatchState match, int playerId)
        {
            List<PlayerActionDefinition> availableActions = new();

            if (match == null || match.progression.ended || match.execution.mode != MatchExecutionMode.WaitingForPlayerAction)
                return availableActions;

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            ExecutionContext context = new()
            {
                actingPlayerId = playerId
            };
            RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);

            if (!DoesPhaseAllowPlayer(match, phase, resolver, playerId))
                return availableActions;

            for (int index = 0; index < phase.availableActions.Length; index++)
            {
                PlayerActionDefinition action = phase.availableActions[index];
                if (ConditionEvaluator.Evaluate(action.when, resolver))
                    availableActions.Add(action);
            }

            return availableActions;
        }

        private List<ReactionDefinition> GetAvailableReactionsForPlayer(MatchState match, PhaseDefinition phase, EventPayload payload, int playerId)
        {
            List<ReactionDefinition> availableReactions = new();
            if (phase.availableReactions == null || phase.availableReactions.Length == 0)
                return availableReactions;

            ExecutionContext context = new()
            {
                actingPlayerId = playerId,
                eventPayload = payload
            };
            RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);

            for (int index = 0; index < phase.availableReactions.Length; index++)
            {
                ReactionDefinition reaction = phase.availableReactions[index];
                if (!DoesParticipantsAllowPlayer(reaction.participants, resolver, playerId, $"Reaction '{reaction.key}'"))
                    continue;

                if (!ConditionEvaluator.Evaluate(reaction.when, resolver))
                    continue;

                availableReactions.Add(reaction);
            }

            return availableReactions;
        }

        private bool TryOpenReactionWindow(MatchState match, PhaseDefinition phase, EventPayload payload)
        {
            if (phase.availableReactions == null || phase.availableReactions.Length == 0)
                return false;

            List<int> eligiblePlayerIds = new();
            for (int index = 0; index < match.players.items.Count; index++)
            {
                int playerId = match.players.items[index].id;
                if (GetAvailableReactionsForPlayer(match, phase, payload, playerId).Count == 0)
                    continue;

                eligiblePlayerIds.Add(playerId);
            }

            if (eligiblePlayerIds.Count == 0)
                return false;

            OpenReactionWindow(match, eligiblePlayerIds, payload.trigger);
            return true;
        }

        private void OpenActionWindow(MatchState match)
        {
            int actorPlayerId = match.interaction.pendingActionPlayerId != RuntimeIds.InvalidId
                ? match.interaction.pendingActionPlayerId
                : match.progression.currentPlayerId;

            InteractionWindow window = new()
            {
                id = match.idCounters.nextInteractionWindowId++,
                kind = InteractionWindowKind.PlayerAction,
                primaryPlayerId = actorPlayerId,
                phaseKey = match.progression.currentPhaseKey
            };
            window.eligiblePlayerIds.Add(actorPlayerId);

            match.interaction.pendingActionPlayerId = RuntimeIds.InvalidId;
            match.execution.mode = MatchExecutionMode.WaitingForPlayerAction;
            match.execution.resolvingPlayerId = RuntimeIds.InvalidId;
            match.execution.hasCurrentEvent = false;
            match.execution.currentEvent = new EventPayload();
            match.interaction.currentWindow = window;

            RecordWaitOpened(match, window, TranscriptStopReason.WaitingForPlayerAction);
            FlushTranscriptBatch(match, TranscriptStopReason.WaitingForPlayerAction, window);
            CaptureCurrentCheckpoint(match);
        }

        private void OpenReactionWindow(MatchState match, List<int> eligiblePlayerIds, string trigger)
        {
            InteractionWindow window = new()
            {
                id = match.idCounters.nextInteractionWindowId++,
                kind = InteractionWindowKind.Reaction,
                primaryPlayerId = RuntimeIds.InvalidId,
                phaseKey = match.progression.currentPhaseKey,
                sourceTrigger = trigger ?? string.Empty
            };

            for (int index = 0; index < eligiblePlayerIds.Count; index++)
                window.eligiblePlayerIds.Add(eligiblePlayerIds[index]);

            match.interaction.pendingActionPlayerId = RuntimeIds.InvalidId;
            match.execution.mode = MatchExecutionMode.WaitingForReaction;
            match.execution.resolvingPlayerId = RuntimeIds.InvalidId;
            match.interaction.currentWindow = window;

            RecordWaitOpened(match, window, TranscriptStopReason.WaitingForReaction);
            FlushTranscriptBatch(match, TranscriptStopReason.WaitingForReaction, window);
            CaptureCurrentCheckpoint(match);
        }

        private void ClearCurrentWindow(MatchState match)
        {
            match.interaction.currentWindow = new InteractionWindow();
        }

        private int ResolveActionWindowPlayerId(MatchState match)
        {
            if (match == null)
                return RuntimeIds.InvalidId;

            InteractionWindow window = GetCurrentInteractionWindow(match);
            if (window.kind == InteractionWindowKind.PlayerAction && window.primaryPlayerId != RuntimeIds.InvalidId)
                return window.primaryPlayerId;

            return match.progression.currentPlayerId;
        }

        private void EnsureWindow(InteractionWindow window, InteractionWindowKind expectedKind, int windowId)
        {
            if (window == null || window.id == RuntimeIds.InvalidId || window.kind != expectedKind)
                throw new InvalidOperationException($"There is no active {expectedKind} interaction window.");

            if (window.id != windowId)
                throw new InvalidOperationException($"Interaction window {windowId} is stale. Current window is {window.id}.");
        }

        private void CaptureCurrentCheckpoint(MatchState match)
        {
            MatchCheckpointMetadata metadata = new()
            {
                interactionWindowId = match.interaction.currentWindow.id,
                mode = match.execution.mode,
                actorPlayerId = match.interaction.currentWindow.primaryPlayerId,
                phaseKey = match.progression.currentPhaseKey,
                sourceTrigger = match.interaction.currentWindow.sourceTrigger
            };

            MatchHistoryTimeline.Capture(match, metadata);
        }

        private bool DoesPhaseAllowPlayer(MatchState match, PhaseDefinition phase, RuntimeBindingResolver resolver, int playerId)
        {
            return DoesParticipantsAllowPlayer(phase.participants, resolver, playerId, $"Phase '{phase.key}'");
        }

        private bool DoesParticipantsAllowPlayer(Value participantsValue, RuntimeBindingResolver resolver, int playerId, string label)
        {
            if (participantsValue == null || participantsValue.IsNull)
                return true;

            Value participants = participantsValue.Resolve(resolver);
            if (participants.kind == ValueKind.PlayerId)
                return participants.idValue == playerId;

            if (participants.kind != ValueKind.Collection || participants.collectionItemKind != ValueKind.PlayerId)
                throw new InvalidOperationException($"{label} participants must resolve to a player id or a collection of player ids.");

            for (int index = 0; index < participants.collectionItems.Count; index++)
            {
                if (participants.collectionItems[index].idValue == playerId)
                    return true;
            }

            return false;
        }
    }
}
