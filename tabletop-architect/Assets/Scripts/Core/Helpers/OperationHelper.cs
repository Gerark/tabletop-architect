using System;
using System.Data.Common;

namespace TTA.Core
{
    public static class OperationHelper
    {
        public static Value GetParam(this OperationDefinition operation, string name, IValueResolver resolver)
        {
            if (!TryGetParam(operation, name, out OperationParameter parameter))
                throw new InvalidOperationException($"Operation '{operation.code}' is missing required parameter '{name}'.");

            return parameter.value.Resolve(resolver);
        }

        private static bool TryGetParam(this OperationDefinition operation, string name, out OperationParameter parameter)
        {
            for (int index = 0; index < operation.parameters.Length; index++)
            {
                if (string.Equals(operation.parameters[index].name, name, StringComparison.OrdinalIgnoreCase))
                {
                    parameter = operation.parameters[index];
                    return true;
                }
            }

            parameter = null;
            return false;
        }

        public static bool HasParam(this OperationDefinition operation, string name)
        {
            for (int index = 0; index < operation.parameters.Length; index++)
            {
                if (string.Equals(operation.parameters[index].name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}