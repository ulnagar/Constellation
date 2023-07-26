namespace Constellation.Infrastructure.ExternalServices.LinkShortener;

using Constellation.Application.Interfaces.Gateways;
using System.Threading.Tasks;

internal sealed class LogOnlyGateway : ILinkShortenerGateway
{
    private readonly ILogger _logger;

    public LogOnlyGateway(ILogger logger)
    {
        _logger = logger.ForContext<ILinkShortenerGateway>();
    }

    public Task<string> ShortenURL(string url) 
    {
        _logger
            .ForContext(nameof(url), url)
            .Information("Call to ShortenURL method");

        return Task.FromResult(url);
    }
}
