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
using System.Net.Mail;
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

    private async Task<Result<EmailMessage>> BuildAndSendEmail(
        string body,
        EmailRecipient from,
        string subject,
        List<string> recipients,
        List<string>? ccRecipients = null,
        List<string>? bccRecipients = null,
        List<Attachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", body);

        EmailMessage message = new()
        {
            From = from,
            SendingModule = string.Empty,
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

        foreach (string entry in ccRecipients ?? [])
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                message.AddRecipient(recipient.Value, EmailRecipientType.Cc);
        }

        foreach (string entry in bccRecipients ?? [])
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                message.AddRecipient(recipient.Value, EmailRecipientType.Bcc);
        }

        Result<string> result = await _emailSender.Send(message, attachments: attachments, cancellationToken: cancellationToken);

        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }

    private async Task<Result<EmailMessage>> BuildAndSendEmail(
        string body,
        EmailRecipient from,
        string subject, 
        List<EmailRecipient> recipients,
        List<EmailRecipient>? ccRecipients = null,
        List<EmailRecipient>? bccRecipients = null,
        List<Attachment>? attachments = null,
        CancellationToken cancellationToken = default) 
    {
        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", body);

        EmailMessage message = new()
        {
            From = from,
            SendingModule = string.Empty,
            Subject = subject,
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (EmailRecipient entry in recipients)
            message.AddRecipient(entry, EmailRecipientType.To);

        foreach (EmailRecipient entry in ccRecipients ?? [])
            message.AddRecipient(entry, EmailRecipientType.Cc);

        foreach (EmailRecipient entry in bccRecipients ?? [])
            message.AddRecipient(entry, EmailRecipientType.Bcc);

        Result<string> result = await _emailSender.Send(message, attachments: attachments, cancellationToken: cancellationToken);
        
        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }

    private async Task<Result<EmailMessage>> BuildAndSendEmail<T>(
        T viewModel, 
        EmailRecipient from, 
        string sendingModule, 
        string subject, 
        List<string> recipients,
        List<string>? ccRecipients = null,
        List<string>? bccRecipients = null,
        List<Attachment>? attachments = null,
        CancellationToken cancellationToken = default) where T : EmailLayoutBaseViewModel
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

        foreach (string entry in ccRecipients ?? [])
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                message.AddRecipient(recipient.Value, EmailRecipientType.Cc);
        }

        foreach (string entry in bccRecipients ?? [])
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                message.AddRecipient(recipient.Value, EmailRecipientType.Bcc);
        }

        _emailRepository.Insert(message);
        await _unitOfWork.CompleteAsync(cancellationToken);

        Result<string> result = await _emailSender.Send(message, attachments: attachments, cancellationToken: cancellationToken);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }

    private async Task<Result<EmailMessage>> BuildAndSendEmail<T>(
        T viewModel, 
        EmailRecipient from, 
        string sendingModule, 
        string subject, 
        List<EmailRecipient> recipients,
        List<EmailRecipient>? ccRecipients = null,
        List<EmailRecipient>? bccRecipients = null,
        List<Attachment>? attachments = null,
        CancellationToken cancellationToken = default) where T : EmailLayoutBaseViewModel
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

        foreach (EmailRecipient entry in ccRecipients ?? [])
            message.AddRecipient(entry, EmailRecipientType.Cc);

        foreach (EmailRecipient entry in bccRecipients ?? [])
            message.AddRecipient(entry, EmailRecipientType.Bcc);

        _emailRepository.Insert(message);
        await _unitOfWork.CompleteAsync(cancellationToken);

        Result<string> result = await _emailSender.Send(message, attachments: attachments, cancellationToken: cancellationToken);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return result.IsSuccess ? message : Result.Failure<EmailMessage>(result.Error);
    }
}
