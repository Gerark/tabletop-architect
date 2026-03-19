using System;
using UnityEditor.PackageManager;
using UnityEngine.UIElements;

namespace TTA.DataDefinition
{
    public enum RandomDistribution
    {
        None,
        Uniform,
        Normal
    }

    public enum ResourceType
    {
        String,
        Number,
        Boolean
    }

    public enum ResourceScope
    {
        Global,
        Player,
        Team
    }

    [Serializable]
    public class GameInfo
    {
        [Serializable]
        public class Duration
        {
            public string name;
            public int min;
            public int max;
        }

        [Serializable]
        public class PlayerCount
        {
            public string name;
            public int min;
            public int max;
        }

        public string name;
        public string capsule;
        public string thumbnail;
        public string background;
        public string[] categories;
        public Duration[] durations;
        public PlayerCount[] playerCounts;
        public int age;

    }

    [Serializable]
    public class ElementFace
    {
        public string id;
        public int value;
    }

    [Serializable]
    public class ElementDefinition
    {
        public ElementFace[] faces;
        public RandomDistribution randomDistribution;
        public bool ownerRequired;
    }

    [Serializable]
    public class ElementPresentation
    {
    }

    [Serializable]
    public class ElementInteraction
    {
    }

    [Serializable]
    public class Element
    {
        public string key;
        public string[] tags;
        public ElementDefinition definition;
        public ElementPresentation presentation;
        public ElementInteraction interaction;
    }

    [Serializable]
    public class Resource
    {
        public string key;
        public ResourceType type;
        public ResourceScope scope;
    }

    [Serializable]
    public class Param
    {
        public string name;
        public Value value;

        public static Param New(string name, Value value)
        {
            return new Param { name = name, value = value };
        }
    }

    [Serializable]
    public class Area
    {
        public string key;
    }

    [Serializable]
    public class LinearPath
    {
        public string key;
        public string[] areas;
        public bool loop;
    }

    [Serializable]
    public class Link
    {
        public string from;
        public string to;
        public string kind;
    }

    [Serializable]
    public class LinkGroup
    {
        public string key;
        public Link[] links;
    }

    [Serializable]
    public class Topology
    {
        public LinearPath[] linearPaths;
        public LinkGroup[] linkGroup;
    }

    [Serializable]
    public class VictoryRule
    {
        public Repeat repeat { get; set; }
        public Condition condition;
        public Value winner;
    }

    [Serializable]
    public class Ruleset
    {
        public string key;
        public Condition when;
        public Setup setup;
        public Play play;
        public VictoryRule[] victoryRules;
    }

    [Serializable]
    public class Play
    {
        public string startPhase;
        public Phase[] phases;
    }

    [Serializable]
    public class Setup
    {
        public Operation[] steps;
    }

    [Serializable]
    public class Repeat
    {
        public Value Collection;
    }

    [Serializable]
    public class Phase
    {
        public string key;
        public Value participants;
        public PlayerAction[] availableActions;
        public EventRule[] events;
    }

    [Serializable]
    public class PlayerAction
    {
        public string action;
        public Param[] parameters;
    }

    [Serializable]
    public sealed class Operation
    {
        public string action;
        public Condition when;
        public Repeat repeat;
        public Param[] parameters;
    }

    [Serializable]
    public sealed class EventRule
    {
        public string trigger;
        public Condition when;
        public string nextPhase;
        public Operation[] operations;
    }

    [Serializable]
    public class GameData
    {
        public GameInfo gameInfo;
        public Resource[] resources;
        public Element[] elements;
        public Area[] areas;
        public Ruleset[] rulesets;
        public Topology topology;
    }
}