using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.API.Serialization;

public sealed class ClienteTipoPersonaJsonConverter : JsonConverter<ClienteTipoPersona>
{
    public override ClienteTipoPersona Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();
            if (TryParse(rawValue, out var tipoPersona))
            {
                return tipoPersona;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
        {
            if (Enum.IsDefined(typeof(ClienteTipoPersona), numericValue))
            {
                return (ClienteTipoPersona)numericValue;
            }
        }

        throw new JsonException("Valor inválido para tipo de persona. Valores permitidos: Natural, Juridica.");
    }

    public override void Write(Utf8JsonWriter writer, ClienteTipoPersona value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    private static bool TryParse(string? rawValue, out ClienteTipoPersona tipoPersona)
    {
        tipoPersona = default;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var normalized = Normalize(rawValue);

        switch (normalized)
        {
            case "natural":
            case "persona_natural":
                tipoPersona = ClienteTipoPersona.Natural;
                return true;
            case "juridica":
            case "persona_juridica":
                tipoPersona = ClienteTipoPersona.Juridica;
                return true;
            default:
                return Enum.TryParse(rawValue, ignoreCase: true, out tipoPersona);
        }
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString()
            .Replace(' ', '_')
            .Replace('-', '_');
    }
}
