namespace Constellation.Infrastructure.Caches.AuthenticatorMetadata;

using Application.Domains.Auth.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Models;
using System.Text;
using System.Text.Json;

public class AuthenticatorMetadataLoader : IAuthenticatorMetadataLoader
{
    private readonly IAuthenticatorMetadataCache _cache;
    private readonly ILogger _logger;
    private readonly string _mdsFilePath;
    private readonly string _communityFilePath;

    public AuthenticatorMetadataLoader(
        IAuthenticatorMetadataCache cache,
        ILogger logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger
            .ForContext<IAuthenticatorMetadataLoader>();

        string dataPath = configuration["Fido2:MetadataPath"]
                          ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

        _mdsFilePath = Path.Combine(dataPath, "fido-mds.jwt");
        _communityFilePath = Path.Combine(dataPath, "community-aaguids.json");
    }

    public void Load()
    {
        Dictionary<Guid, AuthenticatorMetadataEntry> entries = new Dictionary<Guid, AuthenticatorMetadataEntry>();

        LoadFidoMds(entries);
        LoadCommunityList(entries);

        _cache.Load(entries);
        _logger
            .Information("Authenticator metadata cache loaded with {Count} total entries.", entries.Count);
    }

    private void LoadFidoMds(Dictionary<Guid, AuthenticatorMetadataEntry> entries)
    {
        if (!File.Exists(_mdsFilePath))
        {
            _logger
                .Warning("FIDO MDS file not found at {Path}.", _mdsFilePath);

            return;
        }

        try
        {
            string jwt = File.ReadAllText(_mdsFilePath);
            string[] parts = jwt.Split('.');

            if (parts.Length < 2)
            {
                _logger
                    .Error("FIDO MDS file at {Path} is not a valid JWT.", _mdsFilePath);

                return;
            }

            string payloadJson = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(parts[1]));
            MdsPayload? payload = JsonSerializer.Deserialize<MdsPayload>(payloadJson);

            int count = 0;
            foreach (MdsEntry entry in payload?.Entries
                         .Where(e => e.AaGuid is not null
                                     && e.MetadataStatement is not null
                                     && Guid.TryParse(e.AaGuid, out _)) ?? [])
            {
                entries[Guid.Parse(entry.AaGuid!)] = new AuthenticatorMetadataEntry(
                    entry.MetadataStatement!.Description,
                    entry.MetadataStatement.Icon);
                count++;
            }

            _logger
                .Information("Loaded {Count} entries from FIDO MDS.", count);
        }
        catch (Exception ex)
        {
            _logger
                .Error(ex, "Failed to parse FIDO MDS file at {Path}.", _mdsFilePath);
        }
    }

    private void LoadCommunityList(Dictionary<Guid, AuthenticatorMetadataEntry> entries)
    {
        if (!File.Exists(_communityFilePath))
        {
            _logger
                .Warning("Community authenticator list not found at {Path}.", _communityFilePath);
            
            return;
        }

        try
        {
            string json = File.ReadAllText(_communityFilePath);
            Dictionary<string, CommunityAuthenticatorEntry>? community = JsonSerializer.Deserialize<Dictionary<string, CommunityAuthenticatorEntry>>(json);

            int count = 0;
            foreach ((string aaguidStr, CommunityAuthenticatorEntry entry) in community ?? [])
            {
                if (!Guid.TryParse(aaguidStr, out Guid aaguid)) continue;

                // Community list wins on overlapping entries
                entries[aaguid] = new AuthenticatorMetadataEntry(
                    entry.Name,
                    entry.IconLight ?? entry.IconDark);

                count++;
            }

            _logger
                .Information("Merged {Count} entries from community list.", count);
        }
        catch (Exception ex)
        {
            _logger
                .Error(ex, "Failed to parse community authenticator list at {Path}.", _communityFilePath);
        }
    }
}