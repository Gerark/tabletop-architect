using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA
{
    public interface IValueResolver
    {
        Value Resolve(string path);
    }

    [Serializable]
    public enum ValueKind
    {
        Null = 0,
        Int = 1,
        Float = 2,
        Bool = 3,
        String = 4,
        Binding = 5,
        ElementId = 6,
        PlayerId = 7,
        AreaId = 8,
        SlotId = 9,
        Collection = 10
    }

    [JsonConverter(typeof(ValueJsonConverter))]
    public sealed class Value
    {
        public ValueKind kind = ValueKind.Null;

        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue = string.Empty;
        public string bindingPath = string.Empty;
        public int idValue;

        public ValueKind collectionItemKind = ValueKind.Null;
        public List<Value> collectionItems = new();

        public bool IsNull => kind == ValueKind.Null;
        public bool IsBinding => kind == ValueKind.Binding;
        public bool IsCollection => kind == ValueKind.Collection;
        public bool IsNumeric => kind == ValueKind.Int || kind == ValueKind.Float;
        public bool IsRuntimeId =>
            kind == ValueKind.ElementId ||
            kind == ValueKind.PlayerId ||
            kind == ValueKind.AreaId ||
            kind == ValueKind.SlotId;

        public static Value Null()
        {
            return new Value();
        }

        public static Value FromInt(int value)
        {
            return new Value
            {
                kind = ValueKind.Int,
                intValue = value
            };
        }

        public static Value FromFloat(float value)
        {
            return new Value
            {
                kind = ValueKind.Float,
                floatValue = value
            };
        }

        public static Value FromBool(bool value)
        {
            return new Value
            {
                kind = ValueKind.Bool,
                boolValue = value
            };
        }

        public static Value FromString(string value)
        {
            return new Value
            {
                kind = ValueKind.String,
                stringValue = value ?? string.Empty
            };
        }

        public static Value FromBinding(string path)
        {
            return new Value
            {
                kind = ValueKind.Binding,
                bindingPath = path ?? string.Empty
            };
        }

        public static Value FromElementId(int value)
        {
            return FromRuntimeId(ValueKind.ElementId, value);
        }

        public static Value FromPlayerId(int value)
        {
            return FromRuntimeId(ValueKind.PlayerId, value);
        }

        public static Value FromAreaId(int value)
        {
            return FromRuntimeId(ValueKind.AreaId, value);
        }

        public static Value FromSlotId(int value)
        {
            return FromRuntimeId(ValueKind.SlotId, value);
        }

        public static Value FromCollection(IEnumerable<Value> items)
        {
            if (items == null)
            {
                return new Value
                {
                    kind = ValueKind.Collection
                };
            }

            ValueKind itemKind = ValueKind.Null;
            List<Value> copiedItems = new();

            foreach (Value item in items)
            {
                Value nextItem = item == null ? Null() : item.DeepCopy();

                if (nextItem.kind == ValueKind.Collection)
                    throw new InvalidOperationException("Nested collections are not supported.");

                if (itemKind == ValueKind.Null)
                    itemKind = nextItem.kind;
                else if (nextItem.kind != itemKind)
                    throw new InvalidOperationException("Collections must contain homogeneous value kinds.");

                copiedItems.Add(nextItem);
            }

            return new Value
            {
                kind = ValueKind.Collection,
                collectionItemKind = itemKind,
                collectionItems = copiedItems
            };
        }

        public Value Resolve(IValueResolver resolver)
        {
            if (!IsBinding)
                return this;

            if (resolver == null)
                throw new InvalidOperationException("A resolver is required to resolve a binding value.");

            Value current = this;

            for (int i = 0; i < 64; i++)
            {
                if (!current.IsBinding)
                    return current;

                if (string.IsNullOrWhiteSpace(current.bindingPath))
                    throw new InvalidOperationException("Binding path is empty.");

                current = resolver.Resolve(current.bindingPath);

                if (current == null)
                    throw new InvalidOperationException($"Resolver returned null for binding '{bindingPath}'.");
            }

            throw new InvalidOperationException("Too many chained binding resolutions. Possible circular binding.");
        }

        public int AsInt(IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind != ValueKind.Int)
                throw new InvalidOperationException($"Value is {resolved.kind}, expected Int.");

            return resolved.intValue;
        }

        public float AsFloat(IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind == ValueKind.Float)
                return resolved.floatValue;

            if (resolved.kind == ValueKind.Int)
                return resolved.intValue;

            throw new InvalidOperationException($"Value is {resolved.kind}, expected Float or Int.");
        }

        public bool AsBool(IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind != ValueKind.Bool)
                throw new InvalidOperationException($"Value is {resolved.kind}, expected Bool.");

            return resolved.boolValue;
        }

        public string AsString(IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind != ValueKind.String)
                throw new InvalidOperationException($"Value is {resolved.kind}, expected String.");

            return resolved.stringValue ?? string.Empty;
        }

        public int AsRuntimeId(ValueKind expectedKind, IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind != expectedKind)
                throw new InvalidOperationException($"Value is {resolved.kind}, expected {expectedKind}.");

            return resolved.idValue;
        }

        public List<Value> AsCollection(IValueResolver resolver = null)
        {
            Value resolved = ResolveIfNeeded(resolver);

            if (resolved.kind != ValueKind.Collection)
                throw new InvalidOperationException($"Value is {resolved.kind}, expected Collection.");

            List<Value> copiedItems = new(resolved.collectionItems.Count);
            for (int index = 0; index < resolved.collectionItems.Count; index++)
                copiedItems.Add(resolved.collectionItems[index].DeepCopy());

            return copiedItems;
        }

        public Value DeepCopy()
        {
            List<Value> copiedItems = new(collectionItems.Count);
            for (int index = 0; index < collectionItems.Count; index++)
                copiedItems.Add(collectionItems[index].DeepCopy());

            return new Value
            {
                kind = kind,
                intValue = intValue,
                floatValue = floatValue,
                boolValue = boolValue,
                stringValue = stringValue ?? string.Empty,
                bindingPath = bindingPath ?? string.Empty,
                idValue = idValue,
                collectionItemKind = collectionItemKind,
                collectionItems = copiedItems
            };
        }

        private Value ResolveIfNeeded(IValueResolver resolver)
        {
            return kind == ValueKind.Binding ? Resolve(resolver) : this;
        }

        private static Value FromRuntimeId(ValueKind runtimeKind, int value)
        {
            if (runtimeKind != ValueKind.ElementId &&
                runtimeKind != ValueKind.PlayerId &&
                runtimeKind != ValueKind.AreaId &&
                runtimeKind != ValueKind.SlotId)
            {
                throw new InvalidOperationException($"{runtimeKind} is not a runtime id value kind.");
            }

            return new Value
            {
                kind = runtimeKind,
                idValue = value
            };
        }
    }
}
