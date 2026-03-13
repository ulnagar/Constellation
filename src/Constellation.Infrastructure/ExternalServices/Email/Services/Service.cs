namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Models.Messaging.Email;
using Constellation.Core.Models.Messaging.Email.Enums;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Messaging.Email.Repositories;
using Core.ValueObjects;
using System.Runtime.CompilerServices;
using Templates.Views.Shared;

public sealed partial class Service : IEmailService
{
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IEmailRepository _emailRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger _logger;

    public Service(
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IDateTimeProvider dateTime,
        IRazorViewToStringRenderer razorService,
        IEmailRepository emailRepository,
        IUnitOfWork unitOfWork,
        IAppSettingsService appSettings,
        ILogger logger)
    {
        _emailSender = emailSender;
        _calendarService = calendarService;
        _dateTime = dateTime;
        _razorService = razorService;
        _emailRepository = emailRepository;
        _unitOfWork = unitOfWork;
        _appSettings = appSettings;
        _logger = logger.ForContext<IEmailService>();
    }

    private ILogger GetLogger([CallerMemberName] string memberName = "")
    {
        return _logger.ForContext("Action", memberName);
    }

    private async Task<Result<EmailMessage>> BuildAndSendEmail<T>(T viewModel, EmailRecipient from, string sendingModule, string subject, List<string> recipients) where T : EmailLayoutBaseViewModel
    {
        RenderedEmail rendered = await _razorService.RenderEmail(viewModel.ViewLocation, viewModel);

        EmailMessage message = new()
        {
            From = from,
            SendingModule = sendingModule,
            Subject = subject,
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (string entry in recipients)
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                message.AddRecipient(recipient.Value, EmailRecipientType.To);
        }

        _emailRepository.Insert(message);
        await _unitOfWork.CompleteAsync();

        Result<string> result = await _emailSender.Send(message);

        await _unitOfWork.CompleteAsync();

        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }

    private async Task<Result<EmailMessage>> BuildAndSendEmail<T>(T viewModel, EmailRecipient from, string sendingModule, string subject, List<EmailRecipient> recipients) where T : EmailLayoutBaseViewModel
    {
        RenderedEmail rendered = await _razorService.RenderEmail(viewModel.ViewLocation, viewModel);

        EmailMessage message = new()
        {
            From = from,
            SendingModule = sendingModule,
            Subject = subject,
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (EmailRecipient entry in recipients)
            message.AddRecipient(entry, EmailRecipientType.To);

        _emailRepository.Insert(message);
        await _unitOfWork.CompleteAsync();

        Result<string> result = await _emailSender.Send(message);

        await _unitOfWork.CompleteAsync();

        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }
}
