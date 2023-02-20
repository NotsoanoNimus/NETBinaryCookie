using System.Text.Json;
using System.Text.Json.Serialization;

namespace NETBinaryCookie;

public sealed class CookieFlagConverter : JsonConverter<NetBinaryCookie.CookieFlag[]>
{
    public override NetBinaryCookie.CookieFlag[] Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var result = new List<NetBinaryCookie.CookieFlag>();
        
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    result.Add((NetBinaryCookie.CookieFlag)Enum.Parse(typeof(NetBinaryCookie.CookieFlag),
                        reader.GetString()!));
                    break;
            }
        }

        return result.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, NetBinaryCookie.CookieFlag[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Select(x => x.ToString()), options);
    }
}