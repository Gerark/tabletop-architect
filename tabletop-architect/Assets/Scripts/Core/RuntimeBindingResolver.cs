using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed class RepeatFrame
    {
        public bool isActive;
        public Value current = Value.Null();
        public int index = RuntimeIds.InvalidIndex;
    }

    public sealed class RuntimeBindingResolver : IValueResolver
    {
        private readonly GameDefinition _definition;
        private readonly MatchState _match;
        private readonly EventPayload _eventPayload;
        private readonly RepeatFrame _repeatFrame;
        private readonly ValueMap _eventTemps;
        private readonly int _actingPlayerId;

        public RuntimeBindingResolver(
            GameDefinition definition,
            MatchState match,
            EventPayload eventPayload = null,
            RepeatFrame repeatFrame = null,
            ValueMap eventTemps = null,
            int actingPlayerId = RuntimeIds.InvalidId)
        {
            _definition = definition ?? new GameDefinition();
            _match = match ?? throw new ArgumentNullException(nameof(match));
            _eventPayload = eventPayload;
            _repeatFrame = repeatFrame;
            _eventTemps = eventTemps;
            _actingPlayerId = actingPlayerId;
        }

        public Value Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Binding path is empty.");

            string[] segments = path.Split('.');
            if (segments.Length == 0)
                throw new InvalidOperationException("Binding path is empty.");

            switch (segments[0])
            {
                case "Match":
                    return ResolveMatchRoot(segments);
                case "CurrentPlayer":
                    return ResolveCurrentPlayerRoot(segments);
                case "Players":
                    return ResolvePlayersRoot(segments);
                case "repeat":
                    return ResolveRepeatRoot(segments);
                case "Event":
                    return ResolveEventRoot(segments);
                case "Temps":
                    return ResolveMatchTempsRoot(segments, 0);
                default:
                    throw new InvalidOperationException($"Unknown binding root '{segments[0]}'.");
            }
        }

        private Value ResolveMatchRoot(string[] segments)
        {
            if (segments.Length < 2)
                throw new InvalidOperationException("Match bindings must target an exposed property or Temps entry.");

            if (segments[1] == "Temps")
                return ResolveMatchTempsRoot(segments, 1);

            if (segments.Length > 2)
                throw new InvalidOperationException($"Binding '{string.Join(".", segments)}' attempts deep traversal, which is not supported.");

            return _match.properties.GetOrDefault(segments[1]).DeepCopy();
        }

        private Value ResolveCurrentPlayerRoot(string[] segments)
        {
            RuntimePlayerRecord player = GetCurrentPlayer();

            if (segments.Length == 1)
                return Value.FromPlayerId(player.id);

            return ResolvePlayerSegments(player, segments, 1);
        }

        private Value ResolvePlayersRoot(string[] segments)
        {
            if (segments.Length != 1)
                throw new InvalidOperationException("Players bindings only expose the collection itself in the first implementation.");

            List<RuntimePlayerRecord> orderedPlayers = new(_match.players.items);
            orderedPlayers.Sort((left, right) => left.orderIndex.CompareTo(right.orderIndex));

            List<Value> playerIds = new(orderedPlayers.Count);
            for (int index = 0; index < orderedPlayers.Count; index++)
                playerIds.Add(Value.FromPlayerId(orderedPlayers[index].id));

            return Value.FromCollection(playerIds);
        }

        private Value ResolveRepeatRoot(string[] segments)
        {
            if (_repeatFrame == null || !_repeatFrame.isActive)
                throw new InvalidOperationException("repeat bindings are only valid inside an active repeat execution.");

            if (segments.Length < 2)
                throw new InvalidOperationException("repeat bindings must target Current or Index.");

            switch (segments[1])
            {
                case "Index":
                    if (segments.Length != 2)
                        throw new InvalidOperationException("repeat.Index does not support deep traversal.");
                    return Value.FromInt(_repeatFrame.index);
                case "Current":
                    if (segments.Length == 2)
                        return _repeatFrame.current.DeepCopy();
                    return ResolveReferencedOwner(_repeatFrame.current, segments, 2);
                default:
                    throw new InvalidOperationException($"Unknown repeat binding segment '{segments[1]}'.");
            }
        }

        private Value ResolveEventRoot(string[] segments)
        {
            if (_eventPayload == null)
                throw new InvalidOperationException("Event bindings are only valid while resolving an event.");

            if (segments.Length != 2)
                throw new InvalidOperationException("Event bindings only expose direct payload fields in the first implementation.");

            string key = segments[1];
            if (_eventPayload.fields != null && _eventPayload.fields.TryGetValue(key, out Value fieldValue))
                return fieldValue.DeepCopy();

            if (TryResolveCompactEventField(key, out Value compactValue))
                return compactValue;

            return Value.Null();
        }

        private Value ResolveMatchTempsRoot(string[] segments, int offset)
        {
            if (segments.Length != offset + 2)
                throw new InvalidOperationException("Temps bindings only expose direct temp entries in the first implementation.");

            string key = segments[offset + 1];

            if (_eventTemps != null && _eventTemps.TryGetValue(key, out Value eventValue))
                return eventValue.DeepCopy();

            if (_match.temps.turn.TryGetValue(key, out Value turnValue))
                return turnValue.DeepCopy();

            if (_match.temps.setup.TryGetValue(key, out Value setupValue))
                return setupValue.DeepCopy();

            if (_match.temps.match.TryGetValue(key, out Value matchValue))
                return matchValue.DeepCopy();

            return Value.Null();
        }

        private Value ResolvePlayerSegments(RuntimePlayerRecord player, string[] segments, int startIndex)
        {
            if (startIndex >= segments.Length)
                return Value.FromPlayerId(player.id);

            if (segments[startIndex] == "Temps")
            {
                if (startIndex + 1 != segments.Length - 1)
                    throw new InvalidOperationException("Player temp bindings only expose direct entries.");

                return player.temps.GetOrDefault(segments[startIndex + 1]).DeepCopy();
            }

            if (startIndex != segments.Length - 1)
                throw new InvalidOperationException("Deep traversal from runtime ids is not supported.");

            return player.properties.GetOrDefault(segments[startIndex]).DeepCopy();
        }

        private Value ResolveElementSegments(RuntimeElementRecord element, string[] segments, int startIndex)
        {
            if (startIndex >= segments.Length)
                return Value.FromElementId(element.id);

            if (segments[startIndex] == "Temps")
            {
                if (startIndex + 1 != segments.Length - 1)
                    throw new InvalidOperationException("Element temp bindings only expose direct entries.");

                return element.temps.GetOrDefault(segments[startIndex + 1]).DeepCopy();
            }

            if (startIndex != segments.Length - 1)
                throw new InvalidOperationException("Deep traversal from runtime ids is not supported.");

            return element.properties.GetOrDefault(segments[startIndex]).DeepCopy();
        }

        private Value ResolveAreaSegments(RuntimeAreaRecord area, string[] segments, int startIndex)
        {
            if (startIndex >= segments.Length)
                return Value.FromAreaId(area.id);

            if (segments[startIndex] == "Temps")
            {
                if (startIndex + 1 != segments.Length - 1)
                    throw new InvalidOperationException("Area temp bindings only expose direct entries.");

                return area.temps.GetOrDefault(segments[startIndex + 1]).DeepCopy();
            }

            if (startIndex != segments.Length - 1)
                throw new InvalidOperationException("Deep traversal from runtime ids is not supported.");

            return area.properties.GetOrDefault(segments[startIndex]).DeepCopy();
        }

        private Value ResolveReferencedOwner(Value reference, string[] segments, int startIndex)
        {
            switch (reference.kind)
            {
                case ValueKind.PlayerId:
                    return ResolvePlayerSegments(GetPlayer(reference.idValue), segments, startIndex);
                case ValueKind.ElementId:
                    return ResolveElementSegments(GetElement(reference.idValue), segments, startIndex);
                case ValueKind.AreaId:
                    return ResolveAreaSegments(GetArea(reference.idValue), segments, startIndex);
                default:
                    throw new InvalidOperationException($"Value kind {reference.kind} cannot be traversed in bindings.");
            }
        }

        private bool TryResolveCompactEventField(string key, out Value value)
        {
            value = Value.Null();
            if (_eventPayload == null)
                return false;

            if (string.Equals(key, "Trigger", StringComparison.Ordinal))
            {
                value = Value.FromString(_eventPayload.trigger);
                return true;
            }

            if (!_eventPayload.hasMovementData)
                return false;

            if (string.Equals(key, "ElementId", StringComparison.Ordinal))
            {
                value = Value.FromElementId(_eventPayload.movementElementId);
                return true;
            }

            if (string.Equals(key, "RequestedSteps", StringComparison.Ordinal))
            {
                value = Value.FromInt(_eventPayload.movementRequestedSteps);
                return true;
            }

            if (string.Equals(key, "ActualSteps", StringComparison.Ordinal))
            {
                value = Value.FromInt(_eventPayload.movementActualSteps);
                return true;
            }

            if (string.Equals(key, "AreaId", StringComparison.Ordinal))
            {
                value = Value.FromAreaId(_eventPayload.movementAreaId);
                return true;
            }

            if (string.Equals(key, "Area", StringComparison.Ordinal) ||
                string.Equals(key, "AreaKey", StringComparison.Ordinal))
            {
                value = Value.FromString(GetRuntimeAreaKey(_eventPayload.movementAreaId));
                return true;
            }

            if (_eventPayload.movementFinalAreaId != RuntimeIds.InvalidId)
            {
                if (string.Equals(key, "FinalAreaId", StringComparison.Ordinal))
                {
                    value = Value.FromAreaId(_eventPayload.movementFinalAreaId);
                    return true;
                }

                if (string.Equals(key, "FinalAreaKey", StringComparison.Ordinal))
                {
                    value = Value.FromString(GetRuntimeAreaKey(_eventPayload.movementFinalAreaId));
                    return true;
                }
            }

            if (string.Equals(key, "Topology", StringComparison.Ordinal))
            {
                value = Value.FromString(_eventPayload.movementTopologyKey);
                return true;
            }

            if (string.Equals(key, "Link", StringComparison.Ordinal))
            {
                value = Value.FromString(_eventPayload.movementLinkName);
                return true;
            }

            return false;
        }

        private RuntimePlayerRecord GetCurrentPlayer()
        {
            int playerId = _actingPlayerId != RuntimeIds.InvalidId
                ? _actingPlayerId
                : _match.progression.currentPlayerId;

            if (playerId == RuntimeIds.InvalidId)
                throw new InvalidOperationException("The match does not currently have an active player.");

            return GetPlayer(playerId);
        }

        private RuntimePlayerRecord GetPlayer(int playerId)
        {
            for (int index = 0; index < _match.players.items.Count; index++)
            {
                if (_match.players.items[index].id == playerId)
                    return _match.players.items[index];
            }

            throw new InvalidOperationException($"Runtime player {playerId} was not found.");
        }

        private RuntimeElementRecord GetElement(int elementId)
        {
            for (int index = 0; index < _match.elements.items.Count; index++)
            {
                if (_match.elements.items[index].id == elementId)
                    return _match.elements.items[index];
            }

            throw new InvalidOperationException($"Runtime element {elementId} was not found.");
        }

        private RuntimeAreaRecord GetArea(int areaId)
        {
            for (int index = 0; index < _match.areas.items.Count; index++)
            {
                if (_match.areas.items[index].id == areaId)
                    return _match.areas.items[index];
            }

            throw new InvalidOperationException($"Runtime area {areaId} was not found.");
        }

        private string GetRuntimeAreaKey(int areaId)
        {
            if (areaId == RuntimeIds.InvalidId)
                return string.Empty;

            RuntimeAreaRecord area = GetArea(areaId);
            return area.ownerElementId == RuntimeIds.InvalidId
                ? _definition.globalAreas[area.definitionIndex].key ?? string.Empty
                : _definition.elements[GetElement(area.ownerElementId).definitionIndex].ownedAreas[area.definitionIndex].key ?? string.Empty;
        }
    }
}
