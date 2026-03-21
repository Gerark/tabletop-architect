using System;
using System.Collections.Generic;

namespace TTA
{
    public sealed partial class MatchRuntimeController
    {
        private sealed class ExecutionContext
        {
            public EventPayload eventPayload;
            public RepeatFrame repeatFrame;
            public ValueMap eventTemps;

            public RuntimeBindingResolver CreateResolver(GameDefinition definition, MatchState match)
            {
                return new RuntimeBindingResolver(definition, match, eventPayload, repeatFrame, eventTemps);
            }

            public ExecutionContext CreateRepeated(Value current, int index)
            {
                return new ExecutionContext
                {
                    eventPayload = eventPayload,
                    eventTemps = eventTemps,
                    repeatFrame = new RepeatFrame
                    {
                        isActive = true,
                        current = current.DeepCopy(),
                        index = index
                    }
                };
            }
        }

        private sealed class BoxSelection
        {
            public int definitionIndex = RuntimeIds.InvalidIndex;
            public int amount;
        }

        private readonly GameDefinition _definition;
        private readonly Dictionary<string, int> _elementIndicesByKey = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _globalAreaIndicesByKey = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _rulesetIndicesByKey = new(StringComparer.Ordinal);

        public MatchRuntimeController(GameDefinition definition)
        {
            _definition = definition ?? new GameDefinition();
            BuildDefinitionLookups();
        }

        public GameDefinition Definition => _definition;

        public MatchState CreateMatch(string rulesetKey, int playerCount, int seed = 1)
        {
            if (playerCount <= 0)
                throw new InvalidOperationException("A match must have at least one player.");

            RulesetDefinition ruleset = GetRulesetDefinition(rulesetKey);
            MatchState match = new();

            match.progression.rulesetKey = ruleset.key;
            match.random.seed = seed == 0 ? 1 : seed;
            match.random.state = match.random.seed;

            InitializeMatchProperties(match);
            InitializeBoxStock(match);
            CreatePlayers(match, playerCount);
            CreateGlobalAreas(match);

            if (match.players.items.Count > 0)
                match.progression.currentPlayerId = match.players.items[0].id;

            RunSetup(match);
            return match;
        }

        public void RunSetup(MatchState match)
        {
            RulesetDefinition ruleset = GetRulesetDefinition(match.progression.rulesetKey);
            match.execution.mode = MatchExecutionMode.Resolving;

            ExecuteOperations(match, ruleset.setup.steps, new ExecutionContext());

            if (!match.progression.ended && !string.IsNullOrWhiteSpace(ruleset.play.startPhase))
                match.execution.queuedNextPhase = ruleset.play.startPhase;

            FinishResolution(match);
        }

        public IReadOnlyList<PlayerActionDefinition> GetAvailableActions(MatchState match)
        {
            List<PlayerActionDefinition> availableActions = new();

            if (match == null || match.progression.ended || match.execution.mode != MatchExecutionMode.WaitingForPlayerAction)
                return availableActions;

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            RuntimeBindingResolver resolver = new ExecutionContext().CreateResolver(_definition, match);

            if (!DoesPhaseAllowCurrentPlayer(match, phase, resolver))
                return availableActions;

            for (int index = 0; index < phase.availableActions.Length; index++)
            {
                PlayerActionDefinition action = phase.availableActions[index];
                if (ConditionEvaluator.Evaluate(action.when, resolver))
                    availableActions.Add(action);
            }

            return availableActions;
        }

        public void ExecuteAction(MatchState match, string actionKey)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (match.progression.ended)
                throw new InvalidOperationException("The match has already ended.");

            if (match.execution.mode != MatchExecutionMode.WaitingForPlayerAction)
                throw new InvalidOperationException("Player actions can only be executed from a stable waiting state.");

            if (string.IsNullOrWhiteSpace(actionKey))
                throw new InvalidOperationException("Action key is required.");

            PhaseDefinition phase = GetCurrentPhaseDefinition(match);
            RuntimeBindingResolver resolver = new ExecutionContext().CreateResolver(_definition, match);

            if (!DoesPhaseAllowCurrentPlayer(match, phase, resolver))
                throw new InvalidOperationException("The current player is not a participant in the active phase.");

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
                throw new InvalidOperationException($"Action '{actionKey}' is not available.");

            match.execution.mode = MatchExecutionMode.Resolving;
            ExecuteOperations(match, selectedAction.operations, new ExecutionContext());
            FinishResolution(match);
        }

        public bool CanSave(MatchState match)
        {
            if (match == null)
                return false;

            bool noResolutionState =
                !match.execution.hasCurrentEvent &&
                match.execution.queuedEvents.Count == 0 &&
                string.IsNullOrWhiteSpace(match.execution.queuedNextPhase);

            if (match.execution.mode == MatchExecutionMode.WaitingForPlayerAction && !match.progression.ended)
                return noResolutionState;

            if (match.execution.mode == MatchExecutionMode.Ended && match.progression.ended)
                return noResolutionState;

            return false;
        }

        private void BuildDefinitionLookups()
        {
            for (int index = 0; index < _definition.elements.Length; index++)
            {
                string key = _definition.elements[index].key ?? string.Empty;
                if (_elementIndicesByKey.ContainsKey(key))
                    throw new InvalidOperationException($"Duplicate element key '{key}'.");

                _elementIndicesByKey.Add(key, index);
                ValidateAreaDefinitions(_definition.elements[index].ownedAreas, $"element '{key}'");
                ValidateTopologyDefinitions(_definition.elements[index]);
            }

            for (int index = 0; index < _definition.globalAreas.Length; index++)
            {
                string key = _definition.globalAreas[index].key ?? string.Empty;
                if (_globalAreaIndicesByKey.ContainsKey(key))
                    throw new InvalidOperationException($"Duplicate global area key '{key}'.");

                _globalAreaIndicesByKey.Add(key, index);
            }

            ValidateAreaDefinitions(_definition.globalAreas, "game definition");

            for (int index = 0; index < _definition.rulesets.Length; index++)
            {
                string key = _definition.rulesets[index].key ?? string.Empty;
                if (_rulesetIndicesByKey.ContainsKey(key))
                    throw new InvalidOperationException($"Duplicate ruleset key '{key}'.");

                _rulesetIndicesByKey.Add(key, index);
                ValidatePhaseKeys(_definition.rulesets[index]);
            }
        }

        private void FinishResolution(MatchState match)
        {
            while (!match.progression.ended)
            {
                if (string.IsNullOrWhiteSpace(match.progression.currentPhaseKey) &&
                    !string.IsNullOrWhiteSpace(match.execution.queuedNextPhase))
                {
                    string startingPhase = match.execution.queuedNextPhase;
                    match.execution.queuedNextPhase = string.Empty;
                    ApplyPhaseTransition(match, startingPhase);
                    continue;
                }

                DrainEventQueue(match);
                if (match.progression.ended)
                    break;

                if (!string.IsNullOrWhiteSpace(match.execution.queuedNextPhase))
                {
                    string nextPhase = match.execution.queuedNextPhase;
                    match.execution.queuedNextPhase = string.Empty;
                    ApplyPhaseTransition(match, nextPhase);
                    continue;
                }

                break;
            }

            if (!match.progression.ended)
                EvaluateVictory(match);

            if (match.progression.ended)
            {
                ClearResolutionState(match);
                match.execution.mode = MatchExecutionMode.Ended;
                return;
            }

            match.execution.hasCurrentEvent = false;
            match.execution.currentEvent = new EventPayload();
            match.execution.mode = MatchExecutionMode.WaitingForPlayerAction;
        }

        private void DrainEventQueue(MatchState match)
        {
            while (match.execution.queuedEvents.Count > 0 && !match.progression.ended)
            {
                EventPayload payload = match.execution.queuedEvents[0];
                match.execution.queuedEvents.RemoveAt(0);

                match.execution.hasCurrentEvent = true;
                match.execution.currentEvent = payload;

                PhaseDefinition phase = GetCurrentPhaseDefinition(match);
                for (int index = 0; index < phase.events.Length; index++)
                {
                    EventRuleDefinition rule = phase.events[index];
                    if (!string.Equals(rule.trigger, payload.trigger, StringComparison.Ordinal))
                        continue;

                    ExecutionContext context = new()
                    {
                        eventPayload = payload,
                        eventTemps = new ValueMap()
                    };

                    RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);
                    if (!ConditionEvaluator.Evaluate(rule.when, resolver))
                        continue;

                    ExecuteOperations(match, rule.operations, context);
                    if (match.progression.ended)
                        return;

                    if (!string.IsNullOrWhiteSpace(rule.nextPhase))
                        match.execution.queuedNextPhase = rule.nextPhase;
                }

                match.execution.hasCurrentEvent = false;
                match.execution.currentEvent = new EventPayload();
            }
        }

        private void ApplyPhaseTransition(MatchState match, string phaseKey)
        {
            GetPhaseDefinition(GetRulesetDefinition(match.progression.rulesetKey), phaseKey);
            match.progression.currentPhaseKey = phaseKey;

            EventPayload payload = new()
            {
                trigger = "OnPhaseStarted"
            };
            payload.fields.Set("Phase", Value.FromString(phaseKey));
            QueueEvent(match, payload);
        }

        private void EvaluateVictory(MatchState match)
        {
            RulesetDefinition ruleset = GetRulesetDefinition(match.progression.rulesetKey);

            for (int index = 0; index < ruleset.victoryRules.Length; index++)
            {
                VictoryRuleDefinition victoryRule = ruleset.victoryRules[index];
                if (victoryRule.repeat != null && victoryRule.repeat.collection != null && !victoryRule.repeat.collection.IsNull)
                {
                    ExecutionContext context = new();
                    RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);
                    List<Value> repeatedValues = victoryRule.repeat.collection.Resolve(resolver).AsCollection();

                    for (int repeatIndex = 0; repeatIndex < repeatedValues.Count; repeatIndex++)
                    {
                        ExecutionContext repeatedContext = context.CreateRepeated(repeatedValues[repeatIndex], repeatIndex);
                        RuntimeBindingResolver repeatedResolver = repeatedContext.CreateResolver(_definition, match);
                        if (!ConditionEvaluator.Evaluate(victoryRule.condition, repeatedResolver))
                            continue;

                        ApplyVictory(match, victoryRule.winner.Resolve(repeatedResolver));
                        if (match.progression.ended)
                            return;
                    }

                    continue;
                }

                RuntimeBindingResolver baseResolver = new ExecutionContext().CreateResolver(_definition, match);
                if (!ConditionEvaluator.Evaluate(victoryRule.condition, baseResolver))
                    continue;

                ApplyVictory(match, victoryRule.winner.Resolve(baseResolver));
                if (match.progression.ended)
                    return;
            }
        }

        private void ApplyVictory(MatchState match, Value winner)
        {
            if (winner.kind != ValueKind.PlayerId)
                throw new InvalidOperationException("Victory winner must resolve to a player id in the first implementation.");

            match.progression.ended = true;
            match.progression.winnerPlayerId = winner.idValue;
            ClearResolutionState(match);
        }

        private void ClearResolutionState(MatchState match)
        {
            match.execution.queuedEvents.Clear();
            match.execution.queuedNextPhase = string.Empty;
            match.execution.hasCurrentEvent = false;
            match.execution.currentEvent = new EventPayload();
        }

        private bool DoesPhaseAllowCurrentPlayer(MatchState match, PhaseDefinition phase, RuntimeBindingResolver resolver)
        {
            if (phase.participants == null || phase.participants.IsNull)
                return true;

            Value participants = phase.participants.Resolve(resolver);
            if (participants.kind == ValueKind.PlayerId)
                return participants.idValue == match.progression.currentPlayerId;

            if (participants.kind != ValueKind.Collection || participants.collectionItemKind != ValueKind.PlayerId)
                throw new InvalidOperationException($"Phase '{phase.key}' participants must resolve to a player id or a collection of player ids.");

            for (int index = 0; index < participants.collectionItems.Count; index++)
            {
                if (participants.collectionItems[index].idValue == match.progression.currentPlayerId)
                    return true;
            }

            return false;
        }
    }
}
