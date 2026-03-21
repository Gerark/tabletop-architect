using TTA;
using UnityEngine;

public class GameLogicComponent : MonoBehaviour
{
    [SerializeField] private int playerCount = 4;
    [SerializeField] private int seed = 1234;
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float secondsPerAction = 0.35f;

    private Engine _engine;
    private MatchState _match;
    private float _stepCountdown;
    private bool _loggedCompletion;

    void Start()
    {
        StartNewMatch();
    }

    void Update()
    {
        if (_engine == null || _match == null)
            return;

        if (_match.progression.ended)
        {
            if (!_loggedCompletion)
            {
                _loggedCompletion = true;
                Debug.Log($"Match ended. Winner player id: {_match.progression.winnerPlayerId}");
            }

            return;
        }

        if (!autoAdvance)
            return;

        _stepCountdown -= Time.deltaTime;
        if (_stepCountdown > 0f)
            return;

        StepOnce();
        _stepCountdown = Mathf.Max(0.01f, secondsPerAction);
    }

    public void StartNewMatch()
    {
        _engine = new Engine(Engine.CreateMonopolyDefinition());
        _match = _engine.CreateMatch("default_ruleset", Mathf.Max(2, playerCount), seed);
        _stepCountdown = Mathf.Max(0.01f, secondsPerAction);
        _loggedCompletion = false;

        Debug.Log($"Started match. Phase: {_match.progression.currentPhaseKey}, current player id: {_match.progression.currentPlayerId}");
    }

    public void StepOnce()
    {
        if (_engine == null || _match == null || _match.progression.ended)
            return;

        var actions = _engine.GetAvailableActions(_match);
        if (actions.Count == 0)
        {
            Debug.LogWarning($"No available actions in phase '{_match.progression.currentPhaseKey}'.");
            return;
        }

        PlayerActionDefinition action = actions[0];
        Debug.Log($"Executing action '{action.key}' for player {_match.progression.currentPlayerId} in phase '{_match.progression.currentPhaseKey}'.");
        _engine.ExecuteAction(_match, action.key);

        if (_match.progression.ended)
        {
            Debug.Log($"Match ended. Winner player id: {_match.progression.winnerPlayerId}");
            _loggedCompletion = true;
            return;
        }

        Debug.Log($"Resolved to phase '{_match.progression.currentPhaseKey}' for player {_match.progression.currentPlayerId}.");
    }
}
