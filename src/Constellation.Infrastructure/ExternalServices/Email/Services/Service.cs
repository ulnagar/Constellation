namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;

public sealed partial class Service : IEmailService
{
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger _logger;

    public Service(
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IDateTimeProvider dateTime,
        IRazorViewToStringRenderer razorService,
        IAppSettingsService appSettings,
        ILogger logger)
    {
        _emailSender = emailSender;
        _calendarService = calendarService;
        _dateTime = dateTime;
        _razorService = razorService;
        _appSettings = appSettings;
        _logger = logger.ForContext<IEmailService>();
    }
}
