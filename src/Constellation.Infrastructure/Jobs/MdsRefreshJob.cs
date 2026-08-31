namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Domains.Auth.Models;
using Constellation.Infrastructure.Caches.AuthenticatorMetadata;
using Constellation.Infrastructure.Caches.AuthenticatorMetadata.Models;
using Core.Models.Awards.Events;
using Hangfire;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Text.Json;

public class MdsRefreshJob
{
    private readonly IAuthenticatorMetadataCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly string _mdsFilePath;
    private readonly string _communityFilePath;

    private const string MdsUrl = "https://mds.fidoalliance.org/";
    private const string CommunityUrl = "https://raw.githubusercontent.com/passkeydeveloper/passkey-authenticator-aaguids/main/aaguid.json";
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromDays(7);

    public MdsRefreshJob(
        IAuthenticatorMetadataCache cache,
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger
            .ForContext<MdsRefreshJob>();

        string dataPath = configuration["Fido2:MetadataPath"]
                          ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

        Directory.CreateDirectory(dataPath);
        _mdsFilePath = Path.Combine(dataPath, "fido-mds.json");
        _communityFilePath = Path.Combine(dataPath, "community-mds.json");
    }

    // Called by Hangfire on the weekly schedule
    public async Task RefreshAsync(IJobCancellationToken cancellationToken)
    {
        await EnsureFilesAreCurrentAsync(cancellationToken.ShutdownToken);
        LoadIntoMemory();
    }

    // Called directly on startup to populate the cache before any requests are served
    public async Task InitialiseAsync(CancellationToken ct = default)
    {
        await EnsureFilesAreCurrentAsync(ct);
        LoadIntoMemory();
    }

    private async Task EnsureFilesAreCurrentAsync(CancellationToken ct)
    {
        await EnsureFileIsCurrentAsync(_mdsFilePath, () => DownloadMdsAsync(ct), ct);
        await EnsureFileIsCurrentAsync(_communityFilePath, () => DownloadCommunityListAsync(ct), ct);
    }

    private async Task EnsureFileIsCurrentAsync(
        string filePath,
        Func<Task> download,
        CancellationToken ct)
    {
        var fileInfo = new FileInfo(filePath);
        var isStale = !fileInfo.Exists || fileInfo.LastWriteUtcAge() > RefreshInterval;

        if (!isStale) return;

        await download();
    }

    private async Task DownloadMdsAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var response = await client.GetAsync(MdsUrl, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (attempt == maxAttempts)
                {
                    _logger
                        .Error("MDS rate limited after {Attempts} attempts", maxAttempts);
                    return;
                }

                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);

                _logger
                    .Warning("MDS rate limited. Waiting {Delay}s", retryAfter.TotalSeconds);

                await Task.Delay(retryAfter, ct);
                continue;
            }

            response.EnsureSuccessStatusCode();

            string jwt = await response.Content.ReadAsStringAsync(ct);
            var parts = jwt.Split('.');

            if (parts.Length < 2)
            {
                _logger
                    .Error("MDS response is not a valid JWT");
                return;
            }

            var payloadJson = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(parts[1]));
            await File.WriteAllTextAsync(_mdsFilePath, payloadJson, ct);

            _logger.Information("MDS downloaded and saved to {Path}.", _mdsFilePath);
            return;
        }
    }

    private async Task DownloadCommunityListAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = await client.GetStringAsync(CommunityUrl, ct);
            await File.WriteAllTextAsync(_communityFilePath, json, ct);

            _logger.
                Information("Community authenticator list saved to {Path}.", _communityFilePath);
        }
        catch (Exception ex)
        {
            _logger
                .Error(ex, "Failed to download community authenticator list.");
        }
    }

    private void LoadIntoMemory()
    {
        var entries = new Dictionary<Guid, AuthenticatorMetadataEntry>();

        // Load FIDO MDS entries first
        if (File.Exists(_mdsFilePath))
        {
            try
            {
                var json = File.ReadAllText(_mdsFilePath);
                var payload = JsonSerializer.Deserialize<MdsPayload>(json);

                foreach (var entry in payload?.Entries
                    .Where(e => e.AaGuid is not null
                        && e.MetadataStatement is not null
                        && Guid.TryParse(e.AaGuid, out _)) ?? [])
                {
                    entries[Guid.Parse(entry.AaGuid!)] = new AuthenticatorMetadataEntry(
                        entry.MetadataStatement!.Description,
                        entry.MetadataStatement.Icon);
                }

                _logger
                    .Information("Loaded {Count} entries from FIDO MDS.", entries.Count);
            }
            catch (Exception ex)
            {
                _logger
                    .Error(ex, "Failed to parse FIDO MDS file.");
            }
        }

        // Merge community entries -- community list wins on name/icon
        // since it tends to have friendlier display names
        if (File.Exists(_communityFilePath))
        {
            try
            {
                var json = File.ReadAllText(_communityFilePath);
                var community = JsonSerializer.Deserialize<Dictionary<string, CommunityAuthenticatorEntry>>(json);

                var communityCount = 0;
                foreach (var (aaguidStr, entry) in community ?? [])
                {
                    if (!Guid.TryParse(aaguidStr, out var aaguid)) continue;

                    entries[aaguid] = new AuthenticatorMetadataEntry(
                        entry.Name,
                        entry.IconLight ?? entry.IconDark);

                    communityCount++;
                }

                _logger
                    .Information("Merged {Count} entries from community list.", communityCount);
            }
            catch (Exception ex)
            {
                _logger
                    .Error(ex, "Failed to parse community authenticator list.");
            }
        }

        _cache.Load(entries);
        _logger
            .Information("Authenticator metadata cache loaded with {Count} total entries.", entries.Count);
    }
}

internal static class FileInfoExtensions
{
    public static TimeSpan LastWriteUtcAge(this FileInfo fileInfo) =>
        DateTime.UtcNow - fileInfo.LastWriteTimeUtc;
}