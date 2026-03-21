using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA
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

        public static GameDefinition CreateMonopolyDefinition()
        {
            return Sample.CreateMonopolyDefinition();
        }

        public MatchState CreateMatch(string rulesetKey, int playerCount, int seed = 1)
        {
            return _runtime.CreateMatch(rulesetKey, playerCount, seed);
        }

        public IReadOnlyList<PlayerActionDefinition> GetAvailableActions(MatchState match)
        {
            return _runtime.GetAvailableActions(match);
        }

        public void ExecuteAction(MatchState match, string actionKey)
        {
            _runtime.ExecuteAction(match, actionKey);
        }

        public bool CanSave(MatchState match)
        {
            return _runtime.CanSave(match);
        }

        public string SerializeMatch(MatchState match, bool prettyPrint = false)
        {
            if (!CanSave(match))
                throw new InvalidOperationException("Matches can only be serialized at stable save boundaries.");

            return JsonConvert.SerializeObject(
                match,
                prettyPrint ? Formatting.Indented : Formatting.None);
        }

        public MatchState DeserializeMatch(string json)
        {
            MatchState match = JsonConvert.DeserializeObject<MatchState>(json);
            if (match == null)
                throw new InvalidOperationException("Failed to deserialize match state.");

            if (!CanSave(match))
                throw new InvalidOperationException("Serialized matches must represent a stable save boundary.");

            return match;
        }
    }
}
