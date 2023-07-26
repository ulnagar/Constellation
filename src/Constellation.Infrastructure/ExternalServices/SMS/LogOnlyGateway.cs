namespace Constellation.Infrastructure.ExternalServices.SMS;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Serilog;
using System.Threading.Tasks;

internal sealed class LogOnlyGateway : ISMSGateway
{
    private readonly ILogger _logger;

    public LogOnlyGateway(ILogger logger)
    {
        _logger = logger.ForContext<ISMSGateway>();
    }

    public Task<double> GetCreditBalanceAsync()
    {
        _logger.Information("Call to GetCreditBalanceAsync method");

        return Task.FromResult(0d);
    }

    public Task<SMSMessageCollectionDto> SendSmsAsync(object payload) 
    {
        _logger
            .ForContext(nameof(payload), payload, true)
            .Information("Call to SendSmsAsync method");

        return Task.FromResult(new SMSMessageCollectionDto());
    }
}
