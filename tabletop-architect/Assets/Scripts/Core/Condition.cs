using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Core
{
    [Serializable]
    public enum ComparisonOperator
    {
        Eq = 0,
        Neq = 1,
        Gt = 2,
        Gte = 3,
        Lt = 4,
        Lte = 5
    }

    [JsonConverter(typeof(ConditionJsonConverter))]
    public sealed class Condition
    {
        public List<Condition> all = new();
        public List<Condition> any = new();
        public Condition not;
        public ComparisonCondition compare;
    }

    public sealed class ComparisonCondition
    {
        public Value left = Value.Null();
        public ComparisonOperator op;
        public Value right = Value.Null();
    }

    public static class ConditionEvaluator
    {
        public static bool Evaluate(Condition condition, IValueResolver resolver)
        {
            if (condition == null)
                return true;

            bool hasAll = condition.all != null && condition.all.Count > 0;
            bool hasAny = condition.any != null && condition.any.Count > 0;
            bool hasNot = condition.not != null;
            bool hasCompare = condition.compare != null;

            if (!hasAll && !hasAny && !hasNot && !hasCompare)
                return true;

            if (hasAll)
            {
                for (int index = 0; index < condition.all.Count; index++)
                {
                    if (!Evaluate(condition.all[index], resolver))
                        return false;
                }

                return true;
            }

            if (hasAny)
            {
                for (int index = 0; index < condition.any.Count; index++)
                {
                    if (Evaluate(condition.any[index], resolver))
                        return true;
                }

                return false;
            }

            if (hasNot)
                return !Evaluate(condition.not, resolver);

            return EvaluateCompare(condition.compare, resolver);
        }

        private static bool EvaluateCompare(ComparisonCondition compare, IValueResolver resolver)
        {
            Value left = compare.left.Resolve(resolver);
            Value right = compare.right.Resolve(resolver);

            switch (compare.op)
            {
                case ComparisonOperator.Eq:
                    return AreEqual(left, right);
                case ComparisonOperator.Neq:
                    return !AreEqual(left, right);
                case ComparisonOperator.Gt:
                    return left.AsFloat() > right.AsFloat();
                case ComparisonOperator.Gte:
                    return left.AsFloat() >= right.AsFloat();
                case ComparisonOperator.Lt:
                    return left.AsFloat() < right.AsFloat();
                case ComparisonOperator.Lte:
                    return left.AsFloat() <= right.AsFloat();
                default:
                    throw new InvalidOperationException($"Unsupported comparison operator {compare.op}.");
            }
        }

        public static bool AreEqual(Value left, Value right)
        {
            if (left.kind == ValueKind.Int && right.kind == ValueKind.Float)
                return Math.Abs(left.intValue - right.floatValue) < 0.0001f;

            if (left.kind == ValueKind.Float && right.kind == ValueKind.Int)
                return Math.Abs(left.floatValue - right.intValue) < 0.0001f;

            if (left.kind != right.kind)
                return false;

            switch (left.kind)
            {
                case ValueKind.Null:
                    return true;
                case ValueKind.Int:
                    return left.intValue == right.intValue;
                case ValueKind.Float:
                    return Math.Abs(left.floatValue - right.floatValue) < 0.0001f;
                case ValueKind.Bool:
                    return left.boolValue == right.boolValue;
                case ValueKind.String:
                    return string.Equals(left.stringValue, right.stringValue, StringComparison.Ordinal);
                case ValueKind.ElementId:
                case ValueKind.PlayerId:
                case ValueKind.AreaId:
                case ValueKind.SlotId:
                    return left.idValue == right.idValue;
                case ValueKind.Binding:
                    return string.Equals(left.bindingPath, right.bindingPath, StringComparison.Ordinal);
                case ValueKind.Collection:
                    if (left.collectionItems.Count != right.collectionItems.Count ||
                        left.collectionItemKind != right.collectionItemKind)
                    {
                        return false;
                    }

                    for (int index = 0; index < left.collectionItems.Count; index++)
                    {
                        if (!AreEqual(left.collectionItems[index], right.collectionItems[index]))
                            return false;
                    }

                    return true;
                default:
                    throw new InvalidOperationException($"Unsupported ValueKind {left.kind}.");
            }
        }
    }
}
