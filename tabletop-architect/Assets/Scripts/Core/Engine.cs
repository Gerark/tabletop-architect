using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Core
{
    public sealed class Engine
    {
        private readonly GameDefinition _definition;
        private readonly MatchRuntimeController _runtime;

        public Engine()
            : this(new GameDefinition())
        {
        }

        public Engine(GameDefinition definition)
        {
            _definition = definition ?? new GameDefinition();
            _runtime = new MatchRuntimeController(_definition);
        }

        public GameDefinition GetDefinition()
        {
            return _definition;
        }

        public GameDefinition GetData()
        {
            return _definition;
        }

        public MatchState CreateMatch(string rulesetKey, int playerCount, int seed = 1)
        {
            return _runtime.CreateMatch(rulesetKey, playerCount, seed);
        }

        public IReadOnlyList<PlayerActionDefinition> GetAvailableActions(MatchState match)
        {
            return _runtime.GetAvailableActions(match);
        }

        public InteractionWindow GetCurrentInteractionWindow(MatchState match)
        {
            return _runtime.GetCurrentInteractionWindow(match);
        }

        public IReadOnlyList<ReactionDefinition> GetAvailableReactions(MatchState match, int playerId)
        {
            return _runtime.GetAvailableReactions(match, playerId);
        }

        public void ExecuteAction(MatchState match, string actionKey)
        {
            _runtime.ExecuteAction(match, actionKey);
        }

        public void SubmitAction(MatchState match, int windowId, string actionKey)
        {
            _runtime.SubmitAction(match, windowId, actionKey);
        }

        public void SubmitReaction(MatchState match, int windowId, int playerId, string reactionKey)
        {
            _runtime.SubmitReaction(match, windowId, playerId, reactionKey);
        }

        public bool CanUndo(MatchState match)
        {
            return _runtime.CanUndo(match);
        }

        public bool CanRedo(MatchState match)
        {
            return _runtime.CanRedo(match);
        }

        public bool Undo(MatchState match)
        {
            return _runtime.Undo(match);
        }

        public bool Redo(MatchState match)
        {
            return _runtime.Redo(match);
        }

        public bool CanSave(MatchState match)
        {
            return _runtime.CanSave(match);
        }

        public string SerializeMatch(MatchState match, bool prettyPrint = false)
        {
            if (!CanSave(match))
                throw new InvalidOperationException("Matches can only be serialized at stable save boundaries.");

            MatchStateSnapshot snapshot = MatchStateSnapshots.Capture(match);
            return JsonConvert.SerializeObject(
                snapshot,
                prettyPrint ? Formatting.Indented : Formatting.None);
        }

        public MatchState DeserializeMatch(string json)
        {
            MatchStateSnapshot snapshot = JsonConvert.DeserializeObject<MatchStateSnapshot>(json);
            if (snapshot == null)
                throw new InvalidOperationException("Failed to deserialize match state.");

            MatchState match = new();
            MatchStateSnapshots.Restore(match, snapshot);

            if (!CanSave(match))
                throw new InvalidOperationException("Serialized matches must represent a stable save boundary.");

            _runtime.ResetHistory(match);
            return match;
        }
    }
}
