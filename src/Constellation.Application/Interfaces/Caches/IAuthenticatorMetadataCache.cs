namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata;

using Application.Domains.Auth.Models;

public interface IAuthenticatorMetadataCache
{
    AuthenticatorMetadataEntry? Get(Guid aaguid);

    string GetName(Guid aaguid);

    void Load(Dictionary<Guid, AuthenticatorMetadataEntry> entries);
}