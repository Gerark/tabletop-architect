using QFSW.QC;
using System;
using System.Collections.Generic;
using TTA.Core;
using TTA.Presenter;
using UnityEngine;

namespace TTA.Game
{

public class GameLogicComponent : MonoBehaviour
{
    [SerializeField] private GameContentLoadMode contentLoadMode = GameContentLoadMode.CodeDefined;
    [SerializeField] private string contentRootPath = string.Empty;
    [SerializeField] private int playerCount = 4;
    [SerializeField] private int seed = 1234;
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float secondsPerAction = 1.0f;
    [SerializeField] private Dummy3DMaterialPreset[] dummyMaterialPresets = Array.Empty<Dummy3DMaterialPreset>();
    [SerializeField] private Dummy3DTranscriptPresenter dummy3DPresenter;

    private Engine _engine;
    private MatchState _match;
    private LoadedGameContent _loadedContent;
    private LoadedGameContent _configuredContent;
    private readonly TextTranscriptPresenter _transcriptPresenter = new();
    private float _stepCountdown;
    private bool _loggedCompletion;
    private int _presentedTranscriptBatchCount;
    private int _presented3DTranscriptBatchCount;

    void Awake()
    {
        EnsureDummy3DPresenter();
    }

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
        _stepCountdown = secondsPerAction;
    }

    public void StartNewMatch()
    {
        _loadedContent = ResolveGameContent();
        _engine = new Engine(_loadedContent.definition);
        _match = _engine.CreateMatch("default_ruleset", playerCount, seed);
        _stepCountdown = Mathf.Max(0.01f, secondsPerAction);
        _loggedCompletion = false;
        _presentedTranscriptBatchCount = 0;
        _presented3DTranscriptBatchCount = 0;

        Dummy3DTranscriptPresenter presenter = EnsureDummy3DPresenter();
        presenter?.SetMaterialPresets(dummyMaterialPresets);
        presenter?.SetResourceResolver(_loadedContent.resourceResolver);
        presenter?.ResetPresentation(_engine.GetDefinition(), _match);

        Debug.Log($"Started match. Phase: {_match.progression.currentPhaseKey}, current player id: {_match.progression.currentPlayerId}");
        DrainTranscript();
    }

    public void ConfigureCodeContent(
        GameDefinition definition,
        PresentationResourceManifest resources = null,
        string resourceRootPath = "",
        PresentationPackageManifest package = null)
    {
        _configuredContent = GameContentLoader.CreateGameContent(definition, resources, resourceRootPath, package);
    }

    public void ConfigurePackageContent(string packageRootPath)
    {
        _configuredContent = GameContentLoader.LoadFromPackageDirectory(packageRootPath);
    }

    public void StepOnce()
    {
        if (_engine == null || _match == null || _match.progression.ended)
            return;

        InteractionWindow window = _engine.GetCurrentInteractionWindow(_match);
        if (window.kind == InteractionWindowKind.Reaction)
        {
            for (int playerIndex = 0; playerIndex < window.eligiblePlayerIds.Count; playerIndex++)
            {
                int playerId = window.eligiblePlayerIds[playerIndex];
                var reactions = _engine.GetAvailableReactions(_match, playerId);
                if (reactions.Count == 0)
                    continue;

                ReactionDefinition reaction = reactions[0];
                Debug.Log($"Executing reaction '{reaction.key}' for player {playerId} in phase '{_match.progression.currentPhaseKey}'.");
                _engine.SubmitReaction(_match, window.id, playerId, reaction.key);
                FinishStepLogging();
                return;
            }

            Debug.LogWarning($"No available reactions for window {window.id} in phase '{_match.progression.currentPhaseKey}'.");
            return;
        }

        var actions = _engine.GetAvailableActions(_match);
        if (actions.Count == 0)
        {
            Debug.LogWarning($"No available actions in phase '{_match.progression.currentPhaseKey}'.");
            return;
        }

        // Simulate the player taking always the first available action.
        PlayerActionDefinition action = actions[0];
        int actorPlayerId = window.primaryPlayerId != RuntimeIds.InvalidId
            ? window.primaryPlayerId
            : _match.progression.currentPlayerId;
        Debug.Log($"Executing action '{action.key}' for player {actorPlayerId} in phase '{_match.progression.currentPhaseKey}'.");
        _engine.ExecuteAction(_match, action.key);
        FinishStepLogging();
    }

    private void FinishStepLogging()
    {
        DrainTranscript();

        if (_match.progression.ended)
        {
            Debug.Log($"Match ended. Winner player id: {_match.progression.winnerPlayerId}");
            _loggedCompletion = true;
            return;
        }

        Debug.Log($"Resolved to phase '{_match.progression.currentPhaseKey}' for player {_match.progression.currentPlayerId}.");
    }

    private void DrainTranscript()
    {
        List<string> messages = _transcriptPresenter.CollectNewPublicBatches(_match, ref _presentedTranscriptBatchCount);
        for (int index = 0; index < messages.Count; index++)
        {
            QuantumConsole.Instance.LogToConsole(messages[index]);
        }

        EnsureDummy3DPresenter()?.PresentNewPublicBatches(_engine.GetDefinition(), _match, ref _presented3DTranscriptBatchCount);
    }

    private LoadedGameContent ResolveGameContent()
    {
        if (_configuredContent != null)
            return _configuredContent;

        if (contentLoadMode == GameContentLoadMode.PackageFolder)
        {
            if (string.IsNullOrWhiteSpace(contentRootPath))
            {
                Debug.LogWarning("PackageFolder mode requires a content root path. Falling back to code-defined sample content.");
                return CreateSampleContent();
            }

            try
            {
                return GameContentLoader.LoadFromPackageDirectory(contentRootPath);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Failed to load package content from '{contentRootPath}'. Falling back to code-defined sample content.\n{exception}");
                return CreateSampleContent();
            }
        }

        return CreateSampleContent();
    }

    private LoadedGameContent CreateSampleContent()
    {
        return GameContentLoader.CreateGameContent(
            Sample.CreateMonopolyDefinition(),
            Sample.CreateMonopolyResources(),
            contentRootPath);
    }

    private Dummy3DTranscriptPresenter EnsureDummy3DPresenter()
    {
        if (dummy3DPresenter != null)
            return dummy3DPresenter;

        dummy3DPresenter = FindFirstObjectByType<Dummy3DTranscriptPresenter>();
        if (dummy3DPresenter != null)
            return dummy3DPresenter;

        GameObject presenterObject = new("Dummy3DPresenter");
        dummy3DPresenter = presenterObject.AddComponent<Dummy3DTranscriptPresenter>();
        return dummy3DPresenter;
    }
}

}
