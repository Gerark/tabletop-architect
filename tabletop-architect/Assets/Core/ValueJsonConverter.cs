using Unity.Plastic.Newtonsoft.Json;
using System;

namespace TTA
{
    internal class ValueJsonConverter : JsonConverter<Value>
    {
        public override Value ReadJson(JsonReader reader, Type objectType, Value existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject token, got {reader.TokenType}.");
            Value value = new Value();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    return value;
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException($"Expected PropertyName token, got {reader.TokenType}.");
                string propertyName = (string)reader.Value;
                if (!reader.Read())
                    throw new JsonSerializationException("Unexpected end of JSON while reading Value properties.");
                switch (propertyName)
                {
                    case "intValue":
                        value.kind = ValueKind.Int;
                        value.intValue = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "floatValue":
                        value.kind = ValueKind.Float;
                        value.floatValue = reader.Value != null ? Convert.ToSingle(reader.Value) : 0f;
                        break;
                    case "boolValue":
                        value.kind = ValueKind.Bool;
                        value.boolValue = reader.Value != null ? Convert.ToBoolean(reader.Value) : false;
                        break;
                    case "stringValue":
                        value.kind = ValueKind.String;
                        value.stringValue = reader.Value != null ? (string)reader.Value : string.Empty;
                        break;
                    case "bindingPath":
                        value.kind = ValueKind.Binding;
                        value.bindingPath = reader.Value != null ? (string)reader.Value : string.Empty;
                        break;
                    default:
                        // Skip unknown properties
                        reader.Skip();
                        break;
                }
            }
            throw new JsonSerializationException("Unexpected end of JSON while reading Value object.");
        }

        public override void WriteJson(JsonWriter writer, Value value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
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
                case ValueKind.Null:
                default:
                    // No properties to write for null values
                    break;
            }
            writer.WriteEndObject();
        }
    }
}