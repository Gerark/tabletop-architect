using System;
using System.Collections.Generic;

namespace TTA.Core
{
    [Serializable]
    public sealed class NamedValue
    {
        public string key = string.Empty;
        public Value value = Value.Null();
    }

    [Serializable]
    public sealed class ValueMap
    {
        public List<NamedValue> entries = new();

        public bool TryGetValue(string key, out Value value)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                value = entries[index].value;
                return true;
            }

            value = Value.Null();
            return false;
        }

        public Value GetOrDefault(string key)
        {
            return TryGetValue(key, out Value value) ? value : Value.Null();
        }

        public bool Contains(string key)
        {
            return IndexOf(key) >= 0;
        }

        public void Set(string key, Value value)
        {
            int index = IndexOf(key);
            Value copiedValue = value == null ? Value.Null() : value.DeepCopy();

            if (index >= 0)
            {
                entries[index].value = copiedValue;
                return;
            }

            entries.Add(new NamedValue
            {
                key = key ?? string.Empty,
                value = copiedValue
            });
        }

        public bool Remove(string key)
        {
            int index = IndexOf(key);
            if (index < 0)
                return false;

            entries.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            entries.Clear();
        }

        public int Count()
        {
            return entries.Count;
        }

        public ValueMap DeepCopy()
        {
            ValueMap copy = new();
            for (int index = 0; index < entries.Count; index++)
            {
                copy.entries.Add(new NamedValue
                {
                    key = entries[index].key,
                    value = entries[index].value.DeepCopy()
                });
            }

            return copy;
        }

        private int IndexOf(string key)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                if (string.Equals(entries[index].key, key, StringComparison.Ordinal))
                    return index;
            }

            return -1;
        }
    }

    [Serializable]
    public sealed class MatchTempState
    {
        public ValueMap match = new();
        public ValueMap turn = new();
        public ValueMap setup = new();
    }
}
