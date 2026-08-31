namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata.Models;

using System.Text.Json.Serialization;

internal sealed class CommunityAuthenticatorEntry
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("icon_light")]
    public string? IconLight { get; init; }

    [JsonPropertyName("icon_dark")]
    public string? IconDark { get; init; }
}