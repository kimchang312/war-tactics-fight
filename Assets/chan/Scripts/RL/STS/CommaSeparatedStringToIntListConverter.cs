using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class CommaSeparatedStringToIntListConverter : JsonConverter
{
    // 이 컨버터가 List<int> 타입에만 적용되도록
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<int>);
    }

    // JSON → 객체
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartArray:
                // [1,2,3] 형태
                return serializer.Deserialize<List<int>>(reader);

            case JsonToken.String:
                // "1,2,3" 형태
                var s = (string)reader.Value;
                return s
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x.Trim(), out var v) ? v : 0)
                    .ToList();

            case JsonToken.Integer:
            case JsonToken.Float:
                // 단일 숫자 2 또는 2.0 형태
                var single = Convert.ToInt32(reader.Value);
                return new List<int> { single };

            case JsonToken.Null:
                return null;

            default:
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing List<int>");
        }
    }

    // 객체 → JSON
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var list = value as List<int>;
        if (list == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteStartArray();
            foreach (var i in list)
                writer.WriteValue(i);
            writer.WriteEndArray();
        }
    }
}
