namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Interfaces.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[ApiController]
[Route("tiles")]
public class TileProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;
    private readonly IOptions<FileSystemGatewayConfiguration> _configuration;
    private readonly ILogger<TileProxyController> _logger;

    // Respect OSM's tile usage policy - identify your app
    private const string TileBaseUrl = "https://tile.openstreetmap.org";
    private const string UserAgent = "Constellation/1.0 (auroracollegeitsupport@det.nsw.edu.au)";

    public TileProxyController(
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env,
        IOptions<FileSystemGatewayConfiguration> configuration,
        ILogger<TileProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("{z:int}/{x:int}/{y:int}.png")]
    [ResponseCache(Duration = 86400)] // 24h browser cache
    public async Task<IActionResult> GetTile(int z, int x, int y)
    {
        // Validate zoom level to prevent abuse
        if (z < 0 || z > 19)
            return BadRequest();

        var cachePath = GetCachePath(z, x, y);

        if (!System.IO.File.Exists(cachePath))
        {
            if (!await FetchAndCacheTile(z, x, y, cachePath))
                return StatusCode(502, "Failed to fetch tile from upstream.");
        }

        return PhysicalFile(cachePath, "image/png");
    }

    private async Task<bool> FetchAndCacheTile(int z, int x, int y, string cachePath)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("TileProxy");
            var url = $"{TileBaseUrl}/{z}/{x}/{y}.png";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var dir = Path.GetDirectoryName(cachePath)!;
            Directory.CreateDirectory(dir);

            await using var fs = System.IO.File.Create(cachePath);
            await response.Content.CopyToAsync(fs);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch tile {Z}/{X}/{Y}", z, x, y);
            return false;
        }
    }

    private string GetCachePath(int z, int x, int y) =>
        Path.Combine(_configuration.Value.BaseFilePath, "tile-cache", z.ToString(), x.ToString(), $"{y}.png");
}