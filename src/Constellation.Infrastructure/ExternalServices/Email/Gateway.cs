namespace Constellation.Infrastructure.ExternalServices.Email;

using Application.Interfaces.Services;
using Constellation.Application.Interfaces.Gateways;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Email.Identifiers;
using Core.Shared;
using Extensions;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Drawing;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

public class Gateway : IEmailGateway
{
    private readonly IEmailTrackingInjectorService _trackingInjector;
    private readonly EmailGatewayConfiguration _configuration;
    private readonly ILogger _logger;

    private readonly bool _logOnly;

    public Gateway(
        IOptions<EmailGatewayConfiguration> configuration, 
        IEmailTrackingInjectorService trackingInjector,
        ILogger logger)
    {
        _trackingInjector = trackingInjector;
        _logger = logger.ForContext<IEmailGateway>();

        _configuration = configuration.Value;

        _logOnly = !_configuration.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initialised in log only mode");
        }
    }

    public async Task<Result<string>> Send(
        EmailMessage message,
        List<Attachment>? attachments = null,
        string? calendarInfo = null,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        _logger
            .Information("Sending email {id}", message.Id);

        using MimeMessage mime = BuildMimeMessage(message, attachments ?? [], calendarInfo, priority);

        if (_logOnly)
        {
            _logger
                .Information("{id}: Log Only Mode", message.Id);

            return string.Empty;
        }

        _logger
            .Information("{id}: Sending...", message.Id);

        Result<string> result = await PushToServer(message.Id, mime, cancellationToken);

        if (result.IsSuccess)
            message.MarkSent(result.Value);
        else
            message.MarkFailed(result.Error.Message);

        return result;
    }

    private MimeMessage BuildMimeMessage(
        EmailMessage message,
        List<Attachment> attachments,
        string? calendarInfo,
        MessagePriority priority)
    {
        MimeMessage mime = new();

        mime.From.Add(message.From.ToMailboxAddress());

        if (message.ReplyTo is not null)
            mime.ReplyTo.Add(message.ReplyTo.ToMailboxAddress());

        foreach (var recipient in message.Recipients)
        {
            _logger
                .Information("{id}: Adding {name} ({email}) to {type} field.",
                message.Id, recipient.Recipient.Name, recipient.Email, recipient.RecipientType);

            MailboxAddress mailbox = recipient.Recipient.ToMailboxAddress();

            switch (recipient.RecipientType)
            {
                case EmailRecipientType.To: mime.To.Add(mailbox); break;
                case EmailRecipientType.Cc: mime.Cc.Add(mailbox); break;
                case EmailRecipientType.Bcc: mime.Bcc.Add(mailbox); break;
            }
        }

        _logger
            .Information("{id}: Setting Subject to \"{subject}\"", message.Id, message.Subject);
        mime.Subject = message.Subject;
        mime.Priority = priority;

        string trackedHtml = _trackingInjector.InjectTrackingPixel(message.BodyHtml, message.Id);

        TextPart textPartBody = new(TextFormat.Html)
        {
            Text = trackedHtml
        };

        if (attachments.Count > 0 || !string.IsNullOrWhiteSpace(calendarInfo))
        {
            Multipart multipart = new("mixed") { textPartBody };

            foreach (var item in attachments)
            {
                MimePart attachment = new()
                {
                    Content = new MimeContent(item.ContentStream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = item.Name
                };

                _logger.Information("{id}: Adding attachment {name}", message.Id, item.Name);
                multipart.Add(attachment);
            }

            if (!string.IsNullOrWhiteSpace(calendarInfo))
            {
                TextPart ical = new("calendar")
                {
                    ContentTransferEncoding = ContentEncoding.Base64,
                    Text = calendarInfo
                };

                ical.ContentType.Parameters.Add("method", "REQUEST");
                ical.ContentType.Parameters.Add("name", "meeting.ics");

                _logger.Information("{id}: Adding calendar appointment info", message.Id);

                multipart.Add(ical);
                mime.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
            }

            mime.Body = multipart;
        }
        else
        {
            mime.Body = textPartBody;
        }

        return mime;
    }

    private async Task<Result<string>> PushToServer(
        EmailId messageId,
        MimeMessage message,
        CancellationToken cancellationToken = default)
    {
        if (_logOnly)
            return string.Empty;

        using SmtpClient client = new();

        try
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(
                _configuration.Server,
                _configuration.Port,
                SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_configuration.Username))
                await client.AuthenticateAsync(
                    _configuration.Username,
                    _configuration.Password,
                    cancellationToken);

            string response = await client.SendAsync(message, cancellationToken);

            _logger
                .Information("{id}: Email send response: {response}", messageId, response);
        }
        catch (Exception e)
        {
            _logger
                .ForContext(nameof(Exception), e, true)
                .Error("{id}: Email send failed: {response}", messageId, e.Message);

            return Result.Failure<string>(new("Gateway.Email.Failure", e.Message));
        }
        finally
        {
            await client.DisconnectAsync(false, cancellationToken);
        }

        return string.Empty;
    }
}
