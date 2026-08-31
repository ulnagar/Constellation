namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata.Models;

using System.Text.Json.Serialization;

internal sealed class MdsEntry
{
    [JsonPropertyName("aaguid")]
    public string? AaGuid { get; init; }

    [JsonPropertyName("metadataStatement")]
    public MdsMetadataStatement? MetadataStatement { get; init; }
}