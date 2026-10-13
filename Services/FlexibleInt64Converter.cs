using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SipPortal.Services;

/// <summary>
/// USOM API tarih alanlarında sayı, sayısal string, ISO-8601 string veya null
/// dönebilen değerleri güvenli biçimde Int64 olarak okur.
/// null/boş değer 0 kabul edilir.
/// </summary>
public sealed class FlexibleInt64Converter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return 0;

            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var int64))
                    return int64;
                if (reader.TryGetDouble(out var number))
                    return Convert.ToInt64(number, CultureInfo.InvariantCulture);
                break;

            case JsonTokenType.String:
                var text = reader.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return 0;

                if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;

                if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var date))
                    return date.ToUnixTimeSeconds();
                break;
        }

        throw new JsonException($"'{reader.TokenType}' türündeki değer Int64 olarak okunamadı.");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
