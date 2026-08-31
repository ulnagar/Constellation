namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata.Models;

using System.Text.Json.Serialization;

internal sealed class MdsMetadataStatement
{
    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
}