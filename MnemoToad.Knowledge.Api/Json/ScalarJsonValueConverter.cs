using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MnemoToad.Knowledge.Api.Json;

// The built-in JsonValueConverter throws InvalidOperationException (not JsonException) when the
// token is an object/array, which ASP.NET Core's SystemTextJsonInputFormatter doesn't translate into
// a 400 — it only catches JsonException. This converter rejects the same inputs but as a JsonException,
// so a bad attribute value comes back as a normal 400 instead of a 500.
public class ScalarJsonValueConverter : JsonConverter<JsonValue?>
{
    public override JsonValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            throw new JsonException("Attribute values must be a string, number, boolean, or null — objects and arrays are not supported.");

        return JsonValue.Create(document.RootElement.Clone());
    }

    public override void Write(Utf8JsonWriter writer, JsonValue? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            value.WriteTo(writer, options);
    }
}
