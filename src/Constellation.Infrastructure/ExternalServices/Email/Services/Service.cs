namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Microsoft.Extensions.Options;

public sealed partial class Service : IEmailService
{
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly ILogger _logger;
    private readonly AppConfiguration _configuration;

    public Service(
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IDateTimeProvider dateTime,
        IRazorViewToStringRenderer razorService,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _emailSender = emailSender;
        _calendarService = calendarService;
        _dateTime = dateTime;
        _razorService = razorService;
        _logger = logger.ForContext<IEmailService>();
        _configuration = configuration.Value;
    }
}
