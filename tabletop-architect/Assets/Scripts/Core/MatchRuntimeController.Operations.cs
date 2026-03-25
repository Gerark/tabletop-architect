using System;
using System.Collections.Generic;

namespace TTA.Core
{
    public sealed partial class MatchRuntimeController
    {
        private void ExecuteOperations(MatchState match, OperationDefinition[] operations, ExecutionContext context, RuntimeBindingResolver resolver)
        {
            if (operations == null)
                return;

            for (int index = 0; index < operations.Length; index++)
            {
                if (match.progression.ended)
                    return;

                OperationDefinition operation = operations[index];
                if (operation == null)
                    continue;

                if (operation.repeat != null && operation.repeat.collection != null && !operation.repeat.collection.IsNull)
                {
                    ExecuteRepeatedOperation(match, operation, context);
                    continue;
                }

                if (!ConditionEvaluator.Evaluate(operation.when, resolver))
                    continue;

                ExecuteOperationCore(match, operation, context);
            }
        }

        private void ExecuteRepeatedOperation(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            RuntimeBindingResolver resolver = context.CreateResolver(_definition, match);
            List<Value> values = operation.repeat.collection.Resolve(resolver).AsCollection();

            for (int index = 0; index < values.Count; index++)
            {
                if (match.progression.ended)
                    return;

                ExecutionContext repeatedContext = context.CreateRepeated(values[index], index);
                RuntimeBindingResolver repeatedResolver = repeatedContext.CreateResolver(_definition, match);
                if (!ConditionEvaluator.Evaluate(operation.when, repeatedResolver))
                    continue;

                ExecuteOperationCore(match, operation, repeatedContext);
            }
        }

        private void ExecuteOperationCore(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            switch (operation.code)
            {
                case OperationCode.None:
                    return;
                case OperationCode.TakeFromBox:
                    ExecuteTakeFromBox(match, operation, context);
                    return;
                case OperationCode.PlaceElement:
                    ExecutePlaceElement(match, operation, context);
                    return;
                case OperationCode.UnplaceElement:
                    ExecuteUnplaceElement(match, operation, context);
                    return;
                case OperationCode.ReturnToBox:
                    ExecuteReturnToBox(match, operation, context);
                    return;
                case OperationCode.Move:
                    ExecuteMove(match, operation, context);
                    return;
                case OperationCode.WriteProperty:
                    ExecuteWriteProperty(match, operation, context);
                    return;
                case OperationCode.WriteTemp:
                    ExecuteWriteTemp(match, operation, context);
                    return;
                case OperationCode.AdvanceTurn:
                    ExecuteAdvanceTurn(match);
                    return;
                case OperationCode.EndMatch:
                    ExecuteEndMatch(match, operation, context);
                    return;
                case OperationCode.SetFace:
                    ExecuteSetFace(match, operation, context);
                    return;
                case OperationCode.FlipElement:
                    ExecuteFlipElement(match, operation, context);
                    return;
                case OperationCode.Roll:
                    ExecuteRoll(match, operation, context);
                    return;
                case OperationCode.SelectElement:
                    ExecuteSelectElement(match, operation, context);
                    return;
                case OperationCode.DetermineFirstPlayer:
                    ExecuteDetermineFirstPlayer(match, operation, context);
                    return;
                default:
                    throw new InvalidOperationException($"Unsupported operation '{operation.code}'.");
            }
        }

        private void ExecuteTakeFromBox(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<BoxSelection> selections = ResolveBoxSelections(match, operation, resolver);
            int ownerPlayerId = ResolveOptionalOwnerPlayerId(match, operation, resolver);

            List<int> createdElementIds = new();

            for (int selectionIndex = 0; selectionIndex < selections.Count; selectionIndex++)
            {
                BoxSelection selection = selections[selectionIndex];
                BoxStockEntry stockEntry = match.GetBoxStockEntry(selection.definitionIndex);
                if (stockEntry.availableCount < selection.amount)
                {
                    throw new InvalidOperationException($"Insufficient stock for element '{_definition.elements[selection.definitionIndex].key}'.");
                }
            }

            for (int selectionIndex = 0; selectionIndex < selections.Count; selectionIndex++)
            {
                BoxSelection selection = selections[selectionIndex];
                BoxStockEntry stockEntry = match.GetBoxStockEntry(selection.definitionIndex);
                stockEntry.availableCount -= selection.amount;

                for (int countIndex = 0; countIndex < selection.amount; countIndex++)
                {
                    RuntimeElementRecord element = CreateRuntimeElement(match, selection.definitionIndex, ownerPlayerId);
                    match.elements.items.Add(element);
                    createdElementIds.Add(element.id);
                }
            }

            RecordElementsTakenFromBox(match, createdElementIds, ownerPlayerId);

            if (operation.HasParam("Area"))
            {
                int areaId = ResolveAreaId(match, operation, resolver);
                int slotId = ResolveSlotId(match, operation, areaId, resolver);
                PlaceElementIds(match, createdElementIds, areaId, slotId);
                RecordElementsPlaced(match, createdElementIds, areaId, slotId);
            }
        }

        private void ExecutePlaceElement(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);
            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);
            int areaId = ResolveAreaId(match, operation, resolver);
            int slotId = ResolveSlotId(match, operation, areaId, resolver);
            PlaceElementIds(match, elementIds, areaId, slotId);
            RecordElementsPlaced(match, elementIds, areaId, slotId);
        }

        private void ExecuteUnplaceElement(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.placementState == PlacementState.Placed)
                    EnsureOwnedContentEmpty(match, element.id);
            }

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.placementState != PlacementState.Placed)
                    continue;

                RuntimeSlotRecord slot = match.GetSlot(element.slotId);
                RemoveElementFromSlot(match, slot, element.id);

                element.placementState = PlacementState.Unplaced;
                element.areaId = RuntimeIds.InvalidId;
                element.slotId = RuntimeIds.InvalidId;
                element.orderIndex = RuntimeIds.InvalidIndex;

                DestroyOwnedRuntimeContent(match, element.id);
            }

            RecordElementsUnplaced(match, elementIds);
        }

        private void ExecuteReturnToBox(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);
            List<int> elementIndexes = new();

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                if (element.placementState != PlacementState.Unplaced)
                    throw new InvalidOperationException("ReturnToBox only supports unplaced elements.");

                elementIndexes.Add(match.GetElementIndex(element.id));
                match.GetBoxStockEntry(element.definitionIndex).availableCount++;
            }

            elementIndexes.Sort();
            elementIndexes.Reverse();
            for (int index = 0; index < elementIndexes.Count; index++)
                match.elements.items.RemoveAt(elementIndexes[index]);

            RecordElementsReturnedToBox(match, elementIds);
        }

        private void ExecuteMove(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, false, false);
            RuntimeElementRecord element = match.GetElement(elementIds[0]);

            if (element.placementState != PlacementState.Placed)
                throw new InvalidOperationException("Move requires a placed runtime element.");

            int requestedSteps = operation.GetParam("StepAmount", resolver).AsInt();
            if (requestedSteps <= 0)
            {
                throw new InvalidOperationException("Move requires a positive number of steps.");
            }

            string topologyKey = operation.GetParam("Topology", resolver).AsString();
            string linkName = operation.HasParam("Link")
                ? operation.GetParam("Link", resolver).AsString()
                : "Forward";

            RuntimeAreaRecord startArea = match.GetArea(element.areaId);
            RuntimeTopologyRecord topology = match.GetRuntimeTopology(topologyKey, startArea.ownerElementId);
            int startAreaId = startArea.id;

            List<int> traversedAreas = new();
            int currentAreaId = startArea.id;

            for (int stepIndex = 0; stepIndex < requestedSteps; stepIndex++)
            {
                int nextAreaId = RuntimeIds.InvalidId;
                int matches = 0;

                for (int linkIndex = 0; linkIndex < topology.links.Count; linkIndex++)
                {
                    RuntimeTopologyLinkRecord link = topology.links[linkIndex];
                    if (link.fromAreaId == currentAreaId && string.Equals(link.name, linkName, StringComparison.Ordinal))
                    {
                        nextAreaId = link.toAreaId;
                        matches++;
                    }
                }

                if (matches == 0)
                {
                    if (traversedAreas.Count == 0)
                        throw new InvalidOperationException($"Move could not take the first step on topology '{topologyKey}' using link '{linkName}'.");

                    break;
                }

                if (matches > 1)
                    throw new InvalidOperationException($"Move is ambiguous on topology '{topologyKey}' using link '{linkName}'.");

                traversedAreas.Add(nextAreaId);
                currentAreaId = nextAreaId;
            }

            if (traversedAreas.Count == 0)
                throw new InvalidOperationException("Move did not complete any actual steps.");

            int finalAreaId = traversedAreas[traversedAreas.Count - 1];
            int finalSlotId = GetDefaultSlotId(match, finalAreaId);
            PlaceElementIds(match, elementIds, finalAreaId, finalSlotId);

            for (int index = 0; index < traversedAreas.Count - 1; index++)
            {
                QueueEvent(match, CreateMovementPayload(match, "OnAreaPassed", element.id, requestedSteps, traversedAreas.Count, traversedAreas[index], topologyKey, linkName));
            }

            QueueEvent(match, CreateMovementPayload(match, "OnAreaLanded", element.id, requestedSteps, traversedAreas.Count, finalAreaId, topologyKey, linkName));

            EventPayload completedPayload = CreateMovementPayload(match, "OnMovementCompleted", element.id, requestedSteps, traversedAreas.Count, finalAreaId, topologyKey, linkName);
            completedPayload.fields.Set("FinalAreaId", Value.FromAreaId(finalAreaId));
            completedPayload.fields.Set("FinalAreaKey", Value.FromString(GetRuntimeAreaKey(match, finalAreaId)));
            QueueEvent(match, completedPayload);
            RecordElementMoved(match, element.id, startAreaId, finalAreaId, requestedSteps, traversedAreas.Count);
        }

        private void ExecuteWriteProperty(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            string key = operation.GetParam("Key", resolver).AsString();
            Value value = operation.GetParam("Value", resolver);
            string mode = operation.HasParam("Mode")
                ? operation.GetParam("Mode", resolver).AsString()
                : "Set";

            if (!operation.HasParam("Target"))
            {
                ValidatePropertyWrite(PropertyScope.Match, key, value);
                ApplyPropertyWrite(match.properties, key, value, mode);
                return;
            }

            Value target = operation.GetParam("Target", resolver);
            switch (target.kind)
            {
                case ValueKind.PlayerId:
                    ValidatePropertyWrite(PropertyScope.Player, key, value);
                    ApplyPropertyWrite(match.GetPlayer(target.idValue).properties, key, value, mode);
                    return;
                case ValueKind.ElementId:
                    ValidateElementPropertyWrite(match, target.idValue, key, value);
                    ApplyPropertyWrite(match.GetElement(target.idValue).properties, key, value, mode);
                    return;
                case ValueKind.AreaId:
                    ValidateAreaPropertyWrite(match, target.idValue, key, value);
                    ApplyPropertyWrite(match.GetArea(target.idValue).properties, key, value, mode);
                    return;
                default:
                    throw new InvalidOperationException($"WriteProperty does not support target kind '{target.kind}'.");
            }
        }

        private void ExecuteWriteTemp(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            string key = operation.GetParam("Key", resolver).AsString();
            Value value = operation.GetParam("Value", resolver);

            if (!operation.HasParam("Target"))
            {
                string scope = operation.HasParam("Scope")
                    ? operation.GetParam("Scope", resolver).AsString()
                    : "Match";

                switch (scope)
                {
                    case "Event":
                        context.eventTemps ??= new ValueMap();
                        context.eventTemps.Set(key, value);
                        return;
                    case "Turn":
                        match.temps.turn.Set(key, value);
                        return;
                    case "Setup":
                        match.temps.setup.Set(key, value);
                        return;
                    default:
                        match.temps.match.Set(key, value);
                        return;
                }
            }

            Value target = operation.GetParam("Target", resolver);
            switch (target.kind)
            {
                case ValueKind.PlayerId:
                    match.GetPlayer(target.idValue).temps.Set(key, value);
                    return;
                case ValueKind.ElementId:
                    match.GetElement(target.idValue).temps.Set(key, value);
                    return;
                case ValueKind.AreaId:
                    match.GetArea(target.idValue).temps.Set(key, value);
                    return;
                default:
                    throw new InvalidOperationException($"WriteTemp does not support target kind '{target.kind}'.");
            }
        }

        private void ExecuteAdvanceTurn(MatchState match)
        {
            if (match.players.items.Count == 0)
                return;

            List<RuntimePlayerRecord> orderedPlayers = new(match.players.items);
            orderedPlayers.Sort((left, right) => left.orderIndex.CompareTo(right.orderIndex));

            int currentIndex = 0;
            for (int index = 0; index < orderedPlayers.Count; index++)
            {
                if (orderedPlayers[index].id == match.progression.currentPlayerId)
                {
                    currentIndex = index;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % orderedPlayers.Count;
            match.progression.currentPlayerId = orderedPlayers[nextIndex].id;
            match.temps.turn.Clear();
            RecordTurnAdvanced(match, match.progression.currentPlayerId);
        }

        private void ExecuteEndMatch(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            match.progression.ended = true;
            match.progression.winnerPlayerId = RuntimeIds.InvalidId;

            if (operation.HasParam("Winner"))
            {
                Value winner = operation.GetParam("Winner", resolver);
                if (winner.kind == ValueKind.PlayerId)
                    match.progression.winnerPlayerId = winner.idValue;
                else if (!winner.IsNull)
                    throw new InvalidOperationException("EndMatch winner must resolve to a player id.");
            }

            ClearResolutionState(match);
        }

        private void ExecuteSetFace(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);
            string faceId = operation.GetParam("Face", resolver).AsString();

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                element.currentFaceIndex = FindFaceIndex(GetElementDefinition(element), faceId);
            }

            EventPayload payload = new()
            {
                trigger = "OnFaceChanged"
            };
            payload.fields.Set("FaceId", Value.FromString(faceId));
            payload.fields.Set("ElementIds", CreateElementCollection(elementIds));
            QueueEvent(match, payload);
            RecordFaceChanged(match, elementIds, faceId);
        }

        private void ExecuteFlipElement(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);
            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                ElementDefinition definition = GetElementDefinition(element);
                if (definition.faces.Length != 2)
                    throw new InvalidOperationException("FlipElement currently only supports two-face elements.");

                element.currentFaceIndex = element.currentFaceIndex == 0 ? 1 : 0;
            }

            EventPayload payload = new()
            {
                trigger = "OnFaceChanged"
            };
            payload.fields.Set("ElementIds", CreateElementCollection(elementIds));
            QueueEvent(match, payload);

            string faceId = string.Empty;
            if (elementIds.Count > 0)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[0]);
                ElementDefinition definition = GetElementDefinition(element);
                if (element.currentFaceIndex >= 0 && element.currentFaceIndex < definition.faces.Length)
                    faceId = definition.faces[element.currentFaceIndex].id;
            }

            RecordFaceChanged(match, elementIds, faceId);
        }

        private void ExecuteRoll(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            List<int> elementIds = ResolveRuntimeElementIds(match, operation, resolver, true, false);
            List<Value> results = new();
            List<int> rolledValues = new();

            int total = 0;
            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                ElementDefinition definition = GetElementDefinition(element);
                if (definition.faces.Length == 0)
                    throw new InvalidOperationException($"Element '{definition.key}' has no faces to roll.");

                int faceIndex;
                if (definition.faces.Length == 1)
                {
                    faceIndex = 0;
                }
                else
                {
                    if (definition.randomDistribution == RandomDistribution.None)
                        throw new InvalidOperationException($"Element '{definition.key}' does not define a roll distribution.");

                    faceIndex = NextRandomIndex(match, definition.faces.Length);
                }

                element.currentFaceIndex = faceIndex;
                int faceValue = definition.faces[faceIndex].numericValue;
                total += faceValue;
                rolledValues.Add(faceValue);
                results.Add(Value.FromInt(faceValue));
            }

            EventPayload payload = new()
            {
                trigger = "OnRolled"
            };
            payload.fields.Set("Total", Value.FromInt(total));
            payload.fields.Set("Results", Value.FromCollection(results));
            payload.fields.Set("ElementIds", CreateElementCollection(elementIds));
            QueueEvent(match, payload);
            RecordRollResolved(match, elementIds, total, rolledValues);
        }

        private void ExecuteSelectElement(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            Value target = operation.GetParam("Target", resolver);
            if (target.kind != ValueKind.PlayerId)
                throw new InvalidOperationException("SelectElement currently only supports player targets.");

            string assignTo = operation.GetParam("AssignTo", resolver).AsString();
            int ownerPlayerId = target.idValue;

            int definitionIndex;
            if (operation.HasParam("FromTag"))
            {
                string tag = operation.GetParam("FromTag", resolver).AsString();
                definitionIndex = FindFirstAvailableBoxDefinitionByTag(match, tag);
            }
            else if (operation.HasParam("FromKey"))
            {
                string key = operation.GetParam("FromKey", resolver).AsString();
                if (!_elementIndicesByKey.TryGetValue(key, out definitionIndex))
                {
                    throw new InvalidOperationException($"Unknown element key '{key}'.");
                }

                BoxStockEntry stockEntry = match.GetBoxStockEntry(definitionIndex);
                if (stockEntry.availableCount <= 0)
                {
                    throw new InvalidOperationException($"No stock remains for element '{key}'.");
                }
            }
            else
            {
                throw new InvalidOperationException("SelectElement requires FromTag or FromKey.");
            }

            BoxStockEntry selectedStock = match.GetBoxStockEntry(definitionIndex);
            selectedStock.availableCount--;

            RuntimeElementRecord element = CreateRuntimeElement(match, definitionIndex, ownerPlayerId);
            match.elements.items.Add(element);

            Value selectedElement = Value.FromElementId(element.id);
            ValidatePropertyWrite(PropertyScope.Player, assignTo, selectedElement);
            ApplyPropertyWrite(match.GetPlayer(ownerPlayerId).properties, assignTo, selectedElement, "Set");
        }

        private void ExecuteDetermineFirstPlayer(MatchState match, OperationDefinition operation, ExecutionContext context)
        {
            var resolver = context.CreateResolver(_definition, match);

            string method = operation.HasParam("Method")
                ? operation.GetParam("Method", resolver).AsString()
                : "HighestTotal";

            if (!string.Equals(method, "HighestTotal", StringComparison.Ordinal))
                throw new InvalidOperationException($"DetermineFirstPlayer method '{method}' is not supported.");

            List<int> participants = ResolvePlayerIds(operation.GetParam("Participants", resolver));
            if (participants.Count == 0)
                throw new InvalidOperationException("DetermineFirstPlayer requires at least one participant.");

            string tag = operation.GetParam("Tag", resolver).AsString();
            List<int> diceElementIds = FindRuntimeElementIdsByTag(match, tag);
            if (diceElementIds.Count == 0)
                throw new InvalidOperationException($"DetermineFirstPlayer found no runtime elements with tag '{tag}'.");

            int bestPlayerId = participants[0];
            int bestTotal = int.MinValue;

            for (int participantIndex = 0; participantIndex < participants.Count; participantIndex++)
            {
                int total = RollElementsForTotal(match, diceElementIds);
                if (total > bestTotal)
                {
                    bestTotal = total;
                    bestPlayerId = participants[participantIndex];
                }
            }

            match.progression.currentPlayerId = bestPlayerId;
        }

        private List<BoxSelection> ResolveBoxSelections(MatchState match, OperationDefinition operation, IValueResolver resolver)
        {
            List<BoxSelection> selections = new();

            if (operation.HasParam("Key"))
            {
                string key = operation.GetParam("Key", resolver).AsString();
                if (!_elementIndicesByKey.TryGetValue(key, out int definitionIndex))
                    throw new InvalidOperationException($"Unknown element key '{key}'.");

                int amount = operation.HasParam("Amount")
                    ? operation.GetParam("Amount", resolver).AsInt()
                    : 1;

                if (amount <= 0)
                    throw new InvalidOperationException("TakeFromBox amount must be positive.");

                selections.Add(new BoxSelection
                {
                    definitionIndex = definitionIndex,
                    amount = amount
                });

                return selections;
            }

            if (operation.HasParam("Tag"))
            {
                string tag = operation.GetParam("Tag", resolver).AsString();
                for (int index = 0; index < _definition.elements.Length; index++)
                {
                    if (!ElementHasTag(_definition.elements[index], tag))
                        continue;

                    BoxStockEntry stockEntry = match.GetBoxStockEntry(index);
                    if (stockEntry.availableCount <= 0)
                        continue;

                    selections.Add(new BoxSelection
                    {
                        definitionIndex = index,
                        amount = stockEntry.availableCount
                    });
                }

                if (selections.Count == 0)
                    throw new InvalidOperationException($"TakeFromBox found no available stock for tag '{tag}'.");

                return selections;
            }

            throw new InvalidOperationException("TakeFromBox requires a Key or Tag selector.");
        }

        private List<int> ResolveRuntimeElementIds(MatchState match, OperationDefinition operation, IValueResolver resolver, bool allowMany, bool allowZero)
        {
            List<int> ids = new();

            if (operation.HasParam("Target"))
            {
                Value target = operation.GetParam("Target", resolver);
                CollectElementIdsFromValue(target, ids);
            }
            else if (operation.HasParam("Key"))
            {
                string key = operation.GetParam("Key", resolver).AsString();
                if (!_elementIndicesByKey.TryGetValue(key, out int definitionIndex))
                    throw new InvalidOperationException($"Unknown element key '{key}'.");

                for (int index = 0; index < match.elements.items.Count; index++)
                {
                    if (match.elements.items[index].definitionIndex == definitionIndex)
                        ids.Add(match.elements.items[index].id);
                }
            }
            else if (operation.HasParam("Tag"))
            {
                string tag = operation.GetParam("Tag", resolver).AsString();
                for (int index = 0; index < match.elements.items.Count; index++)
                {
                    if (ElementHasTag(GetElementDefinition(match.elements.items[index]), tag))
                        ids.Add(match.elements.items[index].id);
                }
            }
            else
            {
                throw new InvalidOperationException($"Operation '{operation.code}' requires a Target, Key, or Tag selector.");
            }

            EnsureUniqueIds(ids);

            if (!allowZero && ids.Count == 0)
                throw new InvalidOperationException($"Operation '{operation.code}' resolved zero runtime elements.");

            if (!allowMany && ids.Count != 1)
                throw new InvalidOperationException($"Operation '{operation.code}' requires exactly one runtime element.");

            return ids;
        }

        private void CollectElementIdsFromValue(Value target, List<int> ids)
        {
            switch (target.kind)
            {
                case ValueKind.ElementId:
                    ids.Add(target.idValue);
                    return;
                case ValueKind.Collection:
                    if (target.collectionItemKind != ValueKind.ElementId)
                        throw new InvalidOperationException("Selector collections for runtime elements must contain element ids.");

                    for (int index = 0; index < target.collectionItems.Count; index++)
                        ids.Add(target.collectionItems[index].idValue);
                    return;
                default:
                    throw new InvalidOperationException($"Value kind '{target.kind}' cannot select runtime elements.");
            }
        }

        private int ResolveOptionalOwnerPlayerId(MatchState match, OperationDefinition operation, IValueResolver resolver)
        {
            if (!operation.HasParam("Owner"))
                return RuntimeIds.InvalidId;

            Value value = operation.GetParam("Owner", resolver);
            if (value.kind != ValueKind.PlayerId)
                throw new InvalidOperationException("Owner must resolve to a player id.");

            match.GetPlayer(value.idValue);
            return value.idValue;
        }

        private int ResolveAreaId(MatchState match, OperationDefinition operation, IValueResolver resolver)
        {
            Value areaValue = operation.GetParam("Area", resolver);
            bool hasAreaOwner = operation.HasParam("AreaOwner");
            int areaOwnerElementId = hasAreaOwner
                ? ResolveAreaOwnerElementId(match, operation, resolver)
                : RuntimeIds.InvalidId;

            if (areaValue.kind == ValueKind.AreaId)
            {
                RuntimeAreaRecord area = match.GetArea(areaValue.idValue);
                if (hasAreaOwner && area.ownerElementId != areaOwnerElementId)
                    throw new InvalidOperationException("The selected area does not belong to the requested owner element.");

                if (!hasAreaOwner && area.ownerElementId != RuntimeIds.InvalidId)
                    throw new InvalidOperationException("Area id targets an owned area but no AreaOwner was provided.");

                return area.id;
            }

            if (areaValue.kind != ValueKind.String)
                throw new InvalidOperationException("Area parameters must resolve to an area id or area key string.");

            string key = areaValue.stringValue ?? string.Empty;
            int resolvedAreaId = RuntimeIds.InvalidId;

            for (int index = 0; index < match.areas.items.Count; index++)
            {
                RuntimeAreaRecord area = match.areas.items[index];
                if (hasAreaOwner)
                {
                    if (area.ownerElementId != areaOwnerElementId)
                        continue;
                }
                else if (area.ownerElementId != RuntimeIds.InvalidId)
                {
                    continue;
                }

                if (!string.Equals(GetAreaDefinition(match, area).key, key, StringComparison.Ordinal))
                    continue;

                if (resolvedAreaId != RuntimeIds.InvalidId)
                    throw new InvalidOperationException(hasAreaOwner
                        ? $"Area key '{key}' is ambiguous for owner element {areaOwnerElementId}."
                        : $"Global area key '{key}' is ambiguous at runtime.");

                resolvedAreaId = area.id;
            }

            if (resolvedAreaId == RuntimeIds.InvalidId)
            {
                if (hasAreaOwner)
                    throw new InvalidOperationException($"Runtime area '{key}' was not found for owner element {areaOwnerElementId}.");

                throw new InvalidOperationException($"Global runtime area '{key}' was not found.");
            }

            return resolvedAreaId;
        }

        private int ResolveAreaOwnerElementId(MatchState match, OperationDefinition operation, IValueResolver resolver)
        {
            Value ownerValue = operation.GetParam("AreaOwner", resolver);

            switch (ownerValue.kind)
            {
                case ValueKind.ElementId:
                    match.GetElement(ownerValue.idValue);
                    return ownerValue.idValue;
                case ValueKind.String:
                    string ownerKey = ownerValue.stringValue ?? string.Empty;
                    int resolvedOwnerId = RuntimeIds.InvalidId;

                    for (int index = 0; index < match.elements.items.Count; index++)
                    {
                        RuntimeElementRecord element = match.elements.items[index];
                        if (!string.Equals(GetElementDefinition(element).key, ownerKey, StringComparison.Ordinal))
                            continue;

                        if (resolvedOwnerId != RuntimeIds.InvalidId)
                            throw new InvalidOperationException($"AreaOwner key '{ownerKey}' is ambiguous at runtime.");

                        resolvedOwnerId = element.id;
                    }

                    if (resolvedOwnerId == RuntimeIds.InvalidId)
                        throw new InvalidOperationException($"AreaOwner element '{ownerKey}' was not found.");

                    return resolvedOwnerId;
                default:
                    throw new InvalidOperationException("AreaOwner must resolve to an element id or unique runtime element key.");
            }
        }

        private int ResolveSlotId(MatchState match, OperationDefinition operation, int areaId, IValueResolver resolver)
        {
            if (!operation.HasParam("Slot"))
            {
                return GetDefaultSlotId(match, areaId);
            }

            Value slotValue = operation.GetParam("Slot", resolver);
            RuntimeAreaRecord area = match.GetArea(areaId);

            if (slotValue.kind == ValueKind.SlotId)
            {
                RuntimeSlotRecord slot = match.GetSlot(slotValue.idValue);
                if (slot.areaId != area.id)
                    throw new InvalidOperationException("The selected slot does not belong to the target area.");

                return slot.id;
            }

            if (slotValue.kind != ValueKind.String)
                throw new InvalidOperationException("Slot parameters must resolve to a slot id or slot key string.");

            string slotKey = slotValue.stringValue ?? string.Empty;
            int resolvedSlotId = RuntimeIds.InvalidId;

            for (int index = 0; index < area.slotIds.Count; index++)
            {
                RuntimeSlotRecord slot = match.GetSlot(area.slotIds[index]);
                if (!string.Equals(GetSlotDefinition(match, slot).key, slotKey, StringComparison.Ordinal))
                    continue;

                if (resolvedSlotId != RuntimeIds.InvalidId)
                    throw new InvalidOperationException($"Slot key '{slotKey}' is ambiguous inside area '{GetAreaDefinition(match, area).key}'.");

                resolvedSlotId = slot.id;
            }

            if (resolvedSlotId == RuntimeIds.InvalidId)
                throw new InvalidOperationException($"Runtime slot '{slotKey}' was not found in area '{GetAreaDefinition(match, area).key}'.");

            return resolvedSlotId;
        }

        private void ApplyPropertyWrite(ValueMap properties, string key, Value value, string mode)
        {
            if (string.Equals(mode, "Set", StringComparison.Ordinal))
            {
                properties.Set(key, value);
                return;
            }

            if (string.Equals(mode, "Add", StringComparison.Ordinal))
            {
                Value current = properties.GetOrDefault(key);
                if (current.kind != ValueKind.Int || value.kind != ValueKind.Int)
                    throw new InvalidOperationException("WriteProperty in Add mode currently only supports Int properties.");

                properties.Set(key, Value.FromInt(current.intValue + value.intValue));
                return;
            }

            throw new InvalidOperationException($"Unsupported WriteProperty mode '{mode}'.");
        }

        private List<int> ResolvePlayerIds(Value value)
        {
            List<int> playerIds = new();

            switch (value.kind)
            {
                case ValueKind.PlayerId:
                    playerIds.Add(value.idValue);
                    return playerIds;
                case ValueKind.Collection:
                    if (value.collectionItemKind != ValueKind.PlayerId)
                        throw new InvalidOperationException("Expected a player id collection.");

                    for (int index = 0; index < value.collectionItems.Count; index++)
                        playerIds.Add(value.collectionItems[index].idValue);

                    return playerIds;
                default:
                    throw new InvalidOperationException($"Value kind '{value.kind}' cannot resolve to participants.");
            }
        }

        private int FindFirstAvailableBoxDefinitionByTag(MatchState match, string tag)
        {
            for (int definitionIndex = 0; definitionIndex < _definition.elements.Length; definitionIndex++)
            {
                if (!ElementHasTag(_definition.elements[definitionIndex], tag))
                    continue;

                BoxStockEntry stockEntry = match.GetBoxStockEntry(definitionIndex);
                if (stockEntry.availableCount > 0)
                    return definitionIndex;
            }

            throw new InvalidOperationException($"No stock remains for tag '{tag}'.");
        }

        private List<int> FindRuntimeElementIdsByTag(MatchState match, string tag)
        {
            List<int> elementIds = new();
            for (int index = 0; index < match.elements.items.Count; index++)
            {
                if (ElementHasTag(GetElementDefinition(match.elements.items[index]), tag))
                    elementIds.Add(match.elements.items[index].id);
            }

            return elementIds;
        }

        private int RollElementsForTotal(MatchState match, List<int> elementIds)
        {
            int total = 0;

            for (int index = 0; index < elementIds.Count; index++)
            {
                RuntimeElementRecord element = match.GetElement(elementIds[index]);
                ElementDefinition definition = GetElementDefinition(element);
                if (definition.faces.Length == 0)
                    throw new InvalidOperationException($"Element '{definition.key}' has no faces to roll.");

                int faceIndex;
                if (definition.faces.Length == 1)
                {
                    faceIndex = 0;
                }
                else
                {
                    if (definition.randomDistribution == RandomDistribution.None)
                        throw new InvalidOperationException($"Element '{definition.key}' does not define a roll distribution.");

                    faceIndex = NextRandomIndex(match, definition.faces.Length);
                }

                element.currentFaceIndex = faceIndex;
                total += definition.faces[faceIndex].numericValue;
            }

            return total;
        }

        private EventPayload CreateMovementPayload(MatchState match, string trigger, int elementId, int requestedSteps, int actualSteps, int areaId, string topologyKey, string linkName)
        {
            EventPayload payload = new()
            {
                trigger = trigger
            };
            payload.fields.Set("ElementId", Value.FromElementId(elementId));
            payload.fields.Set("RequestedSteps", Value.FromInt(requestedSteps));
            payload.fields.Set("ActualSteps", Value.FromInt(actualSteps));
            payload.fields.Set("AreaId", Value.FromAreaId(areaId));
            payload.fields.Set("Area", Value.FromString(GetRuntimeAreaKey(match, areaId)));
            payload.fields.Set("AreaKey", Value.FromString(GetRuntimeAreaKey(match, areaId)));
            payload.fields.Set("Topology", Value.FromString(topologyKey));
            payload.fields.Set("Link", Value.FromString(linkName));
            return payload;
        }

        private Value CreateElementCollection(List<int> elementIds)
        {
            List<Value> values = new(elementIds.Count);
            for (int index = 0; index < elementIds.Count; index++)
                values.Add(Value.FromElementId(elementIds[index]));

            return Value.FromCollection(values);
        }
    }
}
