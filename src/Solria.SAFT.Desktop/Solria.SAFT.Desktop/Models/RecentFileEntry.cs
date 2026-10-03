using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SolRIA.SAFT.Desktop.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RecentFileType>))]
public enum RecentFileType
{
    Saft,
    Stocks,
    DocumentsAT,
    Transport
}

[JsonConverter(typeof(RecentFileEntryConverter))]
public sealed class RecentFileEntry
{
    public string FullPath { get; set; }
    public RecentFileType FileType { get; set; } = RecentFileType.Saft;
}

// Read the previous string-only history as SAF-T entries.
public sealed class RecentFileEntryConverter : JsonConverter<RecentFileEntry>
{
    public override RecentFileEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new RecentFileEntry { FullPath = reader.GetString() };

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        return new RecentFileEntry
        {
            FullPath = root.GetProperty(nameof(RecentFileEntry.FullPath)).GetString(),
            FileType = root.TryGetProperty(nameof(RecentFileEntry.FileType), out var type)
                ? type.Deserialize<RecentFileType>(options)
                : RecentFileType.Saft
        };
    }

    public override void Write(Utf8JsonWriter writer, RecentFileEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(RecentFileEntry.FullPath), value.FullPath);
        writer.WritePropertyName(nameof(RecentFileEntry.FileType));
        JsonSerializer.Serialize(writer, value.FileType, options);
        writer.WriteEndObject();
    }
}
