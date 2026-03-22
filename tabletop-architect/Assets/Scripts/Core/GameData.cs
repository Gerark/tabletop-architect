using System;

namespace TTA.Core
{
    [Serializable]
    public enum RandomDistribution
    {
        None = 0,
        Uniform = 1,
        Normal = 2
    }

    [Serializable]
    public enum PropertyScope
    {
        Match = 0,
        Player = 1,
        Team = 2,
        Element = 3,
        Area = 4
    }

    [Serializable]
    public enum SlotCapacityKind
    {
        Single = 0,
        Multiple = 1
    }

    [Serializable]
    public enum OperationCode
    {
        None = 0,
        TakeFromBox = 1,
        PlaceElement = 2,
        UnplaceElement = 3,
        ReturnToBox = 4,
        Move = 5,
        WriteProperty = 6,
        WriteTemp = 7,
        AdvanceTurn = 8,
        EndMatch = 9,
        SetFace = 10,
        FlipElement = 11,
        Roll = 12,
        SelectElement = 13,
        DetermineFirstPlayer = 14
    }

    [Serializable]
    public class GameInfo
    {
        [Serializable]
        public sealed class Duration
        {
            public string name = string.Empty;
            public int min;
            public int max;
        }

        [Serializable]
        public sealed class PlayerCount
        {
            public string name = string.Empty;
            public int min;
            public int max;
        }

        public string name = string.Empty;
        public string capsule = string.Empty;
        public string thumbnail = string.Empty;
        public string background = string.Empty;
        public string[] categories = Array.Empty<string>();
        public Duration[] durations = Array.Empty<Duration>();
        public PlayerCount[] playerCounts = Array.Empty<PlayerCount>();
        public int age;
    }

    [Serializable]
    public sealed class ElementFaceDefinition
    {
        public string id = string.Empty;
        public int numericValue;
        public bool isDefault;
    }

    [Serializable]
    public sealed class SlotDefinition
    {
        public string key = "default";
        public bool isDefault = true;
        public SlotCapacityKind capacityKind = SlotCapacityKind.Multiple;
        public int capacityLimit;
    }

    [Serializable]
    public sealed class AreaDefinition
    {
        public string key = string.Empty;
        public PropertyDefinition[] properties = Array.Empty<PropertyDefinition>();
        public SlotDefinition[] slots = Array.Empty<SlotDefinition>();
        public AreaPresentationDefinition presentation = new();
    }

    [Serializable]
    public sealed class LinearPathDefinition
    {
        public string key = string.Empty;
        public string[] areas = Array.Empty<string>();
        public bool loop;
    }

    [Serializable]
    public sealed class TopologyLinkDefinition
    {
        public string from = string.Empty;
        public string to = string.Empty;
        public string name = string.Empty;
    }

    [Serializable]
    public sealed class TopologyLinkGroupDefinition
    {
        public string key = string.Empty;
        public TopologyLinkDefinition[] links = Array.Empty<TopologyLinkDefinition>();
    }

    [Serializable]
    public sealed class TopologyDefinition
    {
        public string key = string.Empty;
        public LinearPathDefinition[] linearPaths = Array.Empty<LinearPathDefinition>();
        public TopologyLinkGroupDefinition[] linkGroups = Array.Empty<TopologyLinkGroupDefinition>();
    }

    [Serializable]
    public sealed class ElementDefinition
    {
        public string key = string.Empty;
        public string[] tags = Array.Empty<string>();
        public int amount = 1;
        public bool ownerRequired;
        public RandomDistribution randomDistribution;
        public ElementFaceDefinition[] faces = Array.Empty<ElementFaceDefinition>();
        public PropertyDefinition[] properties = Array.Empty<PropertyDefinition>();
        public AreaDefinition[] ownedAreas = Array.Empty<AreaDefinition>();
        public TopologyDefinition[] topologies = Array.Empty<TopologyDefinition>();
        public ElementPresentationDefinition presentation = new();
    }

    [Serializable]
    public sealed class PropertyDefinition
    {
        public string key = string.Empty;
        public PropertyScope scope;
        public ValueKind valueKind = ValueKind.Null;
        public Value defaultValue = Value.Null();
    }

    [Serializable]
    public sealed class OperationParameter
    {
        public string name = string.Empty;
        public Value value = Value.Null();

        public static OperationParameter Create(string name, Value value)
        {
            return new OperationParameter
            {
                name = name ?? string.Empty,
                value = value ?? Value.Null()
            };
        }
    }

    [Serializable]
    public sealed class RepeatDefinition
    {
        public Value collection = Value.Null();
    }

    [Serializable]
    public sealed class SetupDefinition
    {
        public OperationDefinition[] steps = Array.Empty<OperationDefinition>();
    }

    [Serializable]
    public sealed class PhaseDefinition
    {
        public string key = string.Empty;
        public Value participants = Value.Null();
        public PlayerActionDefinition[] availableActions = Array.Empty<PlayerActionDefinition>();
        public ReactionDefinition[] availableReactions = Array.Empty<ReactionDefinition>();
        public EventRuleDefinition[] events = Array.Empty<EventRuleDefinition>();
    }

    [Serializable]
    public sealed class PlayerActionDefinition
    {
        public string key = string.Empty;
        public Condition when;
        public OperationDefinition[] operations = Array.Empty<OperationDefinition>();
    }

    [Serializable]
    public sealed class ReactionDefinition
    {
        public string key = string.Empty;
        public Value participants = Value.Null();
        public Condition when;
        public string nextPhase = string.Empty;
        public OperationDefinition[] operations = Array.Empty<OperationDefinition>();
    }

    [Serializable]
    public sealed class OperationDefinition
    {
        public OperationCode code;
        public Condition when;
        public RepeatDefinition repeat;
        public OperationParameter[] parameters = Array.Empty<OperationParameter>();
    }

    [Serializable]
    public sealed class EventRuleDefinition
    {
        public string trigger = string.Empty;
        public Condition when;
        public string nextPhase = string.Empty;
        public OperationDefinition[] operations = Array.Empty<OperationDefinition>();
    }

    [Serializable]
    public sealed class VictoryRuleDefinition
    {
        public RepeatDefinition repeat;
        public Condition condition;
        public Value winner = Value.Null();
    }

    [Serializable]
    public sealed class PlayDefinition
    {
        public string startPhase = string.Empty;
        public PhaseDefinition[] phases = Array.Empty<PhaseDefinition>();
    }

    [Serializable]
    public sealed class RulesetDefinition
    {
        public string key = string.Empty;
        public Condition when;
        public SetupDefinition setup = new();
        public PlayDefinition play = new();
        public VictoryRuleDefinition[] victoryRules = Array.Empty<VictoryRuleDefinition>();
    }

    [Serializable]
    public class GameDefinition
    {
        public GameInfo gameInfo = new();
        public PropertyDefinition[] properties = Array.Empty<PropertyDefinition>();
        public AreaDefinition[] globalAreas = Array.Empty<AreaDefinition>();
        public ElementDefinition[] elements = Array.Empty<ElementDefinition>();
        public RulesetDefinition[] rulesets = Array.Empty<RulesetDefinition>();
    }

    [Serializable]
    public sealed class GameData : GameDefinition
    {
    }
}
