namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata;

using Application.Domains.Auth.Models;
using System;
using System.Collections.Generic;

public sealed class AuthenticatorMetadataCache : IAuthenticatorMetadataCache
{
    private Dictionary<Guid, AuthenticatorMetadataEntry> _cache = [];

    public AuthenticatorMetadataEntry? Get(Guid aaguid) =>
        _cache.TryGetValue(aaguid, out var entry) ? entry : null;

    public string GetName(Guid aaguid) =>
        Get(aaguid)?.Name ?? "Unknown Authenticator";

    public void Load(Dictionary<Guid, AuthenticatorMetadataEntry> entries) =>
        Interlocked.Exchange(ref _cache, entries);
}