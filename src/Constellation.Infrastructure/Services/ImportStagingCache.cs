namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.ImportCache;
using Microsoft.Extensions.Caching.Memory;
using System;

public sealed class ImportStagingCache : IImportStagingCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;
    private static readonly TimeSpan _expiry = TimeSpan.FromMinutes(30);

    public ImportStagingCache(
        IMemoryCache cache, 
        ILogger logger)
    {
        _cache = cache;
        _logger = logger
            .ForContext<IImportStagingCache>();
    }

    public Guid Stage(StagedImport import)
    {
        MemoryCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = _expiry,
            Size = import.Rows.Count,
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (key, _, reason, _) =>
                        _logger
                            .ForContext("Reason", reason)
                            .Information("Import staging entry {Token} evicted", key)
                }
            }
        };

        _cache.Set(CacheKey(import.Token), import, options);
        return import.Token;
    }

    public bool TryGet(Guid token, out StagedImport import) =>
        _cache.TryGetValue(CacheKey(token), out import!);

    public void Remove(Guid token) => _cache.Remove(CacheKey(token));

    private static string CacheKey(Guid token) => $"import-staging:{token}";
}