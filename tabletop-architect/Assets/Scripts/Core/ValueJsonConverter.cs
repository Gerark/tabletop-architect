using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Core
{
    internal sealed class ValueJsonConverter : JsonConverter<Value>
    {
        public override Value ReadJson(JsonReader reader, Type objectType, Value existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Value.Null();

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject token, got {reader.TokenType}.");

            Value value = Value.Null();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    return value;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException($"Expected PropertyName token, got {reader.TokenType}.");

                string propertyName = Convert.ToString(reader.Value) ?? string.Empty;
                if (!reader.Read())
                    throw new JsonSerializationException("Unexpected end of JSON while reading Value.");

                switch (propertyName)
                {
                    case "kind":
                        value.kind = ParseKind(reader.Value);
                        break;
                    case "intValue":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.Int : value.kind;
                        value.intValue = reader.Value == null ? 0 : Convert.ToInt32(reader.Value);
                        break;
                    case "floatValue":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.Float : value.kind;
                        value.floatValue = reader.Value == null ? 0f : Convert.ToSingle(reader.Value);
                        break;
                    case "boolValue":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.Bool : value.kind;
                        value.boolValue = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    case "stringValue":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.String : value.kind;
                        value.stringValue = reader.Value == null ? string.Empty : Convert.ToString(reader.Value);
                        break;
                    case "bindingPath":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.Binding : value.kind;
                        value.bindingPath = reader.Value == null ? string.Empty : Convert.ToString(reader.Value);
                        break;
                    case "idValue":
                        value.idValue = reader.Value == null ? 0 : Convert.ToInt32(reader.Value);
                        break;
                    case "collectionItemKind":
                        value.collectionItemKind = ParseKind(reader.Value);
                        break;
                    case "collectionItems":
                        value.kind = value.kind == ValueKind.Null ? ValueKind.Collection : value.kind;
                        value.collectionItems = serializer.Deserialize<List<Value>>(reader) ?? new List<Value>();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonSerializationException("Unexpected end of JSON while reading Value.");
        }

        public override void WriteJson(JsonWriter writer, Value value, JsonSerializer serializer)
        {
            if (value == null || value.kind == ValueKind.Null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("kind");
            writer.WriteValue(value.kind.ToString());

            switch (value.kind)
            {
                case ValueKind.Int:
                    writer.WritePropertyName("intValue");
                    writer.WriteValue(value.intValue);
                    break;
                case ValueKind.Float:
                    writer.WritePropertyName("floatValue");
                    writer.WriteValue(value.floatValue);
                    break;
                case ValueKind.Bool:
                    writer.WritePropertyName("boolValue");
                    writer.WriteValue(value.boolValue);
                    break;
                case ValueKind.String:
                    writer.WritePropertyName("stringValue");
                    writer.WriteValue(value.stringValue);
                    break;
                case ValueKind.Binding:
                    writer.WritePropertyName("bindingPath");
                    writer.WriteValue(value.bindingPath);
                    break;
                case ValueKind.ElementId:
                case ValueKind.PlayerId:
                case ValueKind.AreaId:
                case ValueKind.SlotId:
                    writer.WritePropertyName("idValue");
                    writer.WriteValue(value.idValue);
                    break;
                case ValueKind.Collection:
                    writer.WritePropertyName("collectionItemKind");
                    writer.WriteValue(value.collectionItemKind.ToString());
                    writer.WritePropertyName("collectionItems");
                    serializer.Serialize(writer, value.collectionItems);
                    break;
            }

            writer.WriteEndObject();
        }

        private static ValueKind ParseKind(object rawValue)
        {
            if (rawValue == null)
                return ValueKind.Null;

            if (rawValue is long longValue)
                return (ValueKind)longValue;

            string stringValue = Convert.ToString(rawValue) ?? string.Empty;
            if (Enum.TryParse(stringValue, true, out ValueKind parsed))
                return parsed;

            return ValueKind.Null;
        }
    }
}
