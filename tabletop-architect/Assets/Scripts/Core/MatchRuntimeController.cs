using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed class ExecutionContext
    {
        public EventPayload eventPayload;
        public RepeatFrame repeatFrame;
        public ValueMap eventTemps;
        public int actingPlayerId = RuntimeIds.InvalidId;

        public RuntimeBindingResolver CreateResolver(GameDefinition definition, MatchState match)
        {
            return new RuntimeBindingResolver(definition, match, eventPayload, repeatFrame, eventTemps, actingPlayerId);
        }

        public ExecutionContext CreateRepeated(Value current, int index)
        {
            return new ExecutionContext
            {
                actingPlayerId = actingPlayerId,
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

    public sealed partial class MatchRuntimeController
    {
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
            BeginTranscriptBatch(match);
            match.execution.mode = MatchExecutionMode.Resolving;

            var context = new ExecutionContext();

            ExecuteOperations(match, ruleset.setup.steps, context, context.CreateResolver(_definition, match));

            if (!match.progression.ended && !string.IsNullOrWhiteSpace(ruleset.play.startPhase))
                match.execution.queuedNextPhase = ruleset.play.startPhase;

            FinishResolution(match);
        }

        public IReadOnlyList<PlayerActionDefinition> GetAvailableActions(MatchState match)
        {
            return GetAvailableActionsForPlayer(match, ResolveActionWindowPlayerId(match));
        }

        public void ExecuteAction(MatchState match, string actionKey)
        {
            SubmitAction(match, GetCurrentInteractionWindow(match).id, actionKey);
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

            if (match.execution.mode == MatchExecutionMode.WaitingForReaction && !match.progression.ended)
                return match.interaction.currentWindow.kind == InteractionWindowKind.Reaction;

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

                if (DrainEventQueue(match))
                    return;

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
                match.ClearWindow();
                match.execution.resolvingPlayerId = RuntimeIds.InvalidId;
                match.execution.mode = MatchExecutionMode.Ended;
                RecordMatchEnded(match);
                FlushTranscriptBatch(match, TranscriptStopReason.MatchEnded, null);
                return;
            }

            OpenActionWindow(match);
        }

        private bool DrainEventQueue(MatchState match)
        {
            while (match.execution.queuedEvents.Count > 0 && !match.progression.ended)
            {
                EventPayload payload = match.execution.queuedEvents[0];
                match.execution.queuedEvents.RemoveAt(0);

                match.execution.hasCurrentEvent = true;
                match.execution.currentEvent = payload;
                RecordEventResolved(match, payload);

                PhaseDefinition phase = GetCurrentPhaseDefinition(match);
                for (int index = 0; index < phase.events.Length; index++)
                {
                    EventRuleDefinition rule = phase.events[index];
                    if (!string.Equals(rule.trigger, payload.trigger, StringComparison.Ordinal))
                        continue;

                    ExecutionContext context = new()
                    {
                        actingPlayerId = match.execution.resolvingPlayerId,
                        eventPayload = payload,
                        eventTemps = new ValueMap()
                    };

                    RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);
                    if (!ConditionEvaluator.Evaluate(rule.when, resolver))
                        continue;

                    ExecuteOperations(match, rule.operations, context, resolver);
                    if (match.progression.ended)
                        return false;

                    if (!string.IsNullOrWhiteSpace(rule.nextPhase))
                        match.execution.queuedNextPhase = rule.nextPhase;
                }

                if (TryOpenReactionWindow(match, phase, payload))
                    return true;

                match.execution.hasCurrentEvent = false;
                match.execution.currentEvent = new EventPayload();
            }

            return false;
        }

        private void ApplyPhaseTransition(MatchState match, string phaseKey)
        {
            GetPhaseDefinition(GetRulesetDefinition(match.progression.rulesetKey), phaseKey);
            match.progression.currentPhaseKey = phaseKey;
            RecordPhaseChanged(match, phaseKey);

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
            match.execution.resolvingPlayerId = RuntimeIds.InvalidId;
            match.execution.hasCurrentEvent = false;
            match.execution.currentEvent = new EventPayload();
        }
    }
}
