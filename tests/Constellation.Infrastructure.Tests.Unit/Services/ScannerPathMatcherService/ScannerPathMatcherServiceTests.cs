namespace Constellation.Infrastructure.Tests.Unit.Services.ScannerPathMatcherService;

using Application.Interfaces.Configuration;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

public class ScannerPathMatcherServiceTests
{
    [Theory]
    [InlineData("/wp-includes/SimplePie/admin.php", true)]
    [InlineData("/Students/Index", false)]
    [InlineData("/xmlrpc.php", true)]
    public void IsBlockedPath_ShouldReturnBool_WithProvidedPaths(string path, bool expected)
    {
        var options = Options.Create(new ScannerBlocklistOptions
        {
            PathFragments = new() { "/wp-", "xmlrpc.php" },
            Extensions = new() { ".php" }
        });

        var sut = new ScannerPathMatcherService(options);

        sut.IsBlockedPath(path).Should().Be(expected);
    }
}
