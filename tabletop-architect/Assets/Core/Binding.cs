using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

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
        Array = 5,
        Object = 6,
        Binding = 7
    }

    [Serializable]
    [JsonConverter(typeof(ValueJsonConverter))]
    public sealed class Value
    {
        public ValueKind kind = ValueKind.Null;

        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue = string.Empty;

        public string bindingPath = string.Empty;

        public bool IsBinding => kind == ValueKind.Binding;
        public bool IsLiteral => kind != ValueKind.Binding;
        public bool IsNull => kind == ValueKind.Null;
        public bool IsNumber => kind == ValueKind.Int || kind == ValueKind.Float;

        public Value() { }

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

        public Value Resolve(IValueResolver resolver)
        {
            if (kind != ValueKind.Binding)
                return this;

            if (resolver == null)
                throw new InvalidOperationException("A resolver is required to resolve a binding value.");

            Value current = this;

            // Safety guard in case a binding resolves to another binding in a loop.
            for (int i = 0; i < 64; i++)
            {
                if (current.kind != ValueKind.Binding)
                    return current;

                if (string.IsNullOrEmpty(current.bindingPath))
                    throw new InvalidOperationException("Binding path is empty.");

                current = resolver.Resolve(current.bindingPath);

                if (current == null)
                    throw new InvalidOperationException($"Resolver returned null for binding '{bindingPath}'.");
            }

            throw new InvalidOperationException("Too many chained binding resolutions. Possible circular binding.");
        }

        public T Get<T>(IValueResolver resolver = null)
        {
            Value resolved = kind == ValueKind.Binding ? Resolve(resolver) : this;
            Type requestedType = typeof(T);

            if (requestedType == typeof(int))
            {
                if (resolved.kind != ValueKind.Int)
                    throw new InvalidOperationException($"Value is {resolved.kind}, expected Int.");
                return (T)(object)resolved.intValue;
            }

            if (requestedType == typeof(float))
            {
                if (resolved.kind == ValueKind.Float)
                    return (T)(object)resolved.floatValue;

                if (resolved.kind == ValueKind.Int)
                    return (T)(object)(float)resolved.intValue;

                throw new InvalidOperationException($"Value is {resolved.kind}, expected Float or Int.");
            }

            if (requestedType == typeof(bool))
            {
                if (resolved.kind != ValueKind.Bool)
                    throw new InvalidOperationException($"Value is {resolved.kind}, expected Bool.");
                return (T)(object)resolved.boolValue;
            }

            if (requestedType == typeof(string))
            {
                if (resolved.kind != ValueKind.String)
                    throw new InvalidOperationException($"Value is {resolved.kind}, expected String.");
                return (T)(object)resolved.stringValue;
            }

            if (requestedType == typeof(Value))
            {
                return (T)(object)resolved;
            }

            throw new NotSupportedException($"Type {requestedType.Name} is not supported by Value.Get<T>().");
        }

        public Value DeepCopy()
        {
            switch (kind)
            {
                case ValueKind.Null:
                    return Null();

                case ValueKind.Int:
                    return FromInt(intValue);

                case ValueKind.Float:
                    return FromFloat(floatValue);

                case ValueKind.Bool:
                    return FromBool(boolValue);

                case ValueKind.String:
                    return FromString(stringValue);

                case ValueKind.Binding:
                    return FromBinding(bindingPath);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string ToUnityJson(bool prettyPrint = false)
        {
            switch(kind)
            {
                case ValueKind.Null:
                case ValueKind.Int:
                case ValueKind.Float:
                case ValueKind.Bool:
                case ValueKind.String:
                    return JsonUtility.ToJson(this, prettyPrint);
                case ValueKind.Binding:
                    // For bindings, we serialize a simple object with the binding path for easier readability.
                    return JsonUtility.ToJson(new { bind = bindingPath }, prettyPrint);
                default:
                    throw new InvalidOperationException($"Unsupported ValueKind: {kind}");
            }
        }
    }

    public sealed class SimpleValueResolver : IValueResolver
    {
        private readonly Dictionary<string, Value> _values = new();

        public void Set(string path, Value value)
        {
            _values[path] = value;
        }

        public Value Resolve(string path)
        {
            if (_values.TryGetValue(path, out Value value))
                return value;

            throw new KeyNotFoundException($"Binding path '{path}' was not found.");
        }
    }
}