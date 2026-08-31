namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

internal sealed class MdsPayload
{
    [JsonPropertyName("nextUpdate")]
    public string NextUpdate { get; init; }

    [JsonPropertyName("entries")]
    public List<MdsEntry> Entries { get; init; } = [];
}