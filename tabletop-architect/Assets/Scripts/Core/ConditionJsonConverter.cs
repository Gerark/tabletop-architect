using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Core
{
    internal sealed class ConditionJsonConverter : JsonConverter<Condition>
    {
        public override Condition ReadJson(JsonReader reader, Type objectType, Condition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject token, got {reader.TokenType}.");

            Condition condition = new();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    return IsEmpty(condition) ? null : condition;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException($"Expected PropertyName token, got {reader.TokenType}.");

                string propertyName = Convert.ToString(reader.Value) ?? string.Empty;
                if (!reader.Read())
                    throw new JsonSerializationException("Unexpected end of JSON while reading Condition.");

                switch (propertyName)
                {
                    case "all":
                        condition.all = serializer.Deserialize<List<Condition>>(reader) ?? new List<Condition>();
                        break;
                    case "any":
                        condition.any = serializer.Deserialize<List<Condition>>(reader) ?? new List<Condition>();
                        break;
                    case "not":
                        condition.not = serializer.Deserialize<Condition>(reader);
                        break;
                    case "compare":
                        condition.compare = serializer.Deserialize<ComparisonCondition>(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonSerializationException("Unexpected end of JSON while reading Condition.");
        }

        public override void WriteJson(JsonWriter writer, Condition value, JsonSerializer serializer)
        {
            if (value == null || IsEmpty(value))
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            if (value.all != null && value.all.Count > 0)
            {
                writer.WritePropertyName("all");
                serializer.Serialize(writer, value.all);
            }

            if (value.any != null && value.any.Count > 0)
            {
                writer.WritePropertyName("any");
                serializer.Serialize(writer, value.any);
            }

            if (value.not != null)
            {
                writer.WritePropertyName("not");
                serializer.Serialize(writer, value.not);
            }

            if (value.compare != null)
            {
                writer.WritePropertyName("compare");
                serializer.Serialize(writer, value.compare);
            }

            writer.WriteEndObject();
        }

        private static bool IsEmpty(Condition value)
        {
            if (value == null)
                return true;

            bool hasAll = value.all != null && value.all.Count > 0;
            bool hasAny = value.any != null && value.any.Count > 0;
            bool hasNot = value.not != null;
            bool hasCompare = value.compare != null;
            return !hasAll && !hasAny && !hasNot && !hasCompare;
        }
    }
}
