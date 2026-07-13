namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Interfaces.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Shared.Extensions;

public static class TileProxyEndpoints
{
    private static readonly ILogger _logger = Log.Logger.ForContext(typeof(TileProxyEndpoints)).ForStaffPortal();
    private const string _tileBaseUrl = "https://tile.openstreetmap.org";
    private static readonly TimeSpan _maxCacheAge = TimeSpan.FromHours(24);
    
    public static void MapTileProxyEndpoints(this WebApplication app)
    {
        app.MapGet("/api/tiles/{z}/{x}/{y}.png", GetTile)
           .CacheOutput(p => p.Expire(TimeSpan.FromDays(1)));
    }

    private static async Task<IResult> GetTile(
        int z, int x, int y,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env,
        IOptions<FileSystemGatewayConfiguration> configuration)
    {
        if (z is < 0 or > 19)
            return Results.BadRequest();

        string cachePath = GetCachePath(z, x, y, env, configuration);
        
        if (IsCacheStale(cachePath))
        {
            if (!await FetchAndCacheTile(z, x, y, cachePath, httpClientFactory))
                return Results.Problem("Failed to fetch tile from upstream.", statusCode: 502);
        }

        return Results.File(cachePath, "image/png");
    }

    private static bool IsCacheStale(string cachePath)
    {
        if (!File.Exists(cachePath))
            return true;

        return DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath) > _maxCacheAge;
    }

    private static async Task<bool> FetchAndCacheTile(
        int z, int x, int y,
        string cachePath,
        IHttpClientFactory httpClientFactory)
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient("TileProxy");
            HttpResponseMessage response = await client.GetAsync($"{_tileBaseUrl}/{z}/{x}/{y}.png");
            response.EnsureSuccessStatusCode();

            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

            await using FileStream fs = File.Create(cachePath);
            await response.Content.CopyToAsync(fs);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to fetch tile {Z}/{X}/{Y}", z, x, y);
            return false;
        }
    }

    private static string GetCachePath(
        int z, int x, int y, 
        IWebHostEnvironment env,
        IOptions<FileSystemGatewayConfiguration> configuration) =>
        Path.Combine(configuration.Value.BaseFilePath, "Mapping/tile-cache", z.ToString(), x.ToString(), $"{y}.png");
}