using System;
using System.Collections.Generic;

namespace TTA
{
    public enum ComparisonOperator
    {
        Eq,
        Neq,
        Gt,
        Gte,
        Lt,
        Lte
    }

    public sealed class Condition
    {
        // Composite
        public List<Condition> All { get; set; }
        public List<Condition> Any { get; set; }
        public Condition Not { get; set; }

        // Leaf
        public ComparisonCondition Compare { get; set; }
    }

    public sealed class ComparisonCondition
    {
        public Value Left { get; set; }
        public ComparisonOperator Op { get; set; }
        public Value Right { get; set; }

        public ComparisonCondition()
        {
        }
    }

    public static class ConditionEvaluator
    {
        public static bool Evaluate(Condition condition, IValueResolver resolver)
        {
            if (condition.All is not null)
            {
                foreach (Condition child in condition.All)
                {
                    if (!Evaluate(child, resolver))
                        return false;
                }
                return true;
            }

            if (condition.Any is not null)
            {
                foreach (Condition child in condition.Any)
                {
                    if (Evaluate(child, resolver))
                        return true;
                }
                return false;
            }

            if (condition.Not is not null)
            {
                return !Evaluate(condition.Not, resolver);
            }

            if (condition.Compare is not null)
            {
                return EvaluateCompare(condition.Compare, resolver);
            }

            throw new InvalidOperationException("Condition is invalid.");
        }

        private static bool EvaluateCompare(ComparisonCondition compare, IValueResolver resolver)
        {
            Value left = compare.Left.Resolve(resolver);
            Value right = compare.Right.Resolve(resolver);

            return compare.Op switch
            {
                ComparisonOperator.Eq => AreEqual(left, right),
                ComparisonOperator.Neq => !AreEqual(left, right),
                ComparisonOperator.Gt => left.Get<float>(resolver) > right.Get<float>(resolver),
                ComparisonOperator.Gte => left.Get<float>(resolver) >= right.Get<float>(resolver),
                ComparisonOperator.Lt => left.Get<float>(resolver) < right.Get<float>(resolver),
                ComparisonOperator.Lte => left.Get<float>(resolver) <= right.Get<float>(resolver),
                _ => throw new InvalidOperationException()
            };
        }

        private static bool AreEqual(Value a, Value b)
        {
            if (a.kind != b.kind)
                return false;

            return a.kind switch
            {
                ValueKind.Int => a.Get<int>() == b.Get<int>(),
                ValueKind.Float => a.Get<float>() == b.Get<float>(),
                ValueKind.Bool => a.Get<bool>() == b.Get<bool>(),
                ValueKind.String => a.Get<string>() == b.Get<string>(),
                _ => false
            };
        }
    }
}