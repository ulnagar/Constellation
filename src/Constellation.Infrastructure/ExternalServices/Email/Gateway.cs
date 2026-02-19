namespace Constellation.Infrastructure.ExternalServices.Email;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.ValueObjects;
using Core.Shared;
using Extensions;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

public class Gateway : IEmailGateway
{
    private readonly EmailGatewayConfiguration _configuration;
    private readonly ILogger _logger;

    private readonly bool _logOnly;

    public Gateway(
        IOptions<EmailGatewayConfiguration> configuration, 
        ILogger logger)
    {
        _logger = logger.ForContext<IEmailGateway>();

        _configuration = configuration.Value;

        _logOnly = !_configuration.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initialised in log only mode");
        }
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        EmailRecipient fromRecipient,
        string subject,
        string body,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            [],
            [],
            fromRecipient,
            subject,
            body,
            [],
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        string fromAddress,
        string subject,
        string body,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            [],
            [],
            fromAddress,
            subject,
            body,
            [],
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            [],
            [],
            fromAddress,
            subject,
            body,
            attachments,
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            [],
            fromAddress,
            subject,
            body,
            [],
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        EmailRecipient fromAddress,
        string subject,
        string body,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            [],
            fromAddress,
            subject,
            body,
            [],
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            [],
            fromAddress,
            subject,
            body,
            attachments,
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            [],
            fromAddress,
            subject,
            body,
            attachments,
            calendarInfo,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            bccRecipients,
            fromAddress,
            subject,
            body,
            [],
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            bccRecipients,
            fromAddress,
            subject,
            body,
            attachments,
            string.Empty,
            priority,
            cancellationToken);
    }

    public Task<Result<MimeMessage>> Send(
        List<EmailRecipient> toRecipients,
        EmailRecipient fromRecipient,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            [],
            [],
            fromRecipient,
            subject,
            body,
            attachments,
            string.Empty,
            priority,
            cancellationToken);
    }

    private async Task<Result<MimeMessage>> SendAll(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();

        _logger.Information("Sending email {id}", id);

        MimeMessage message = new();

        message.From.Add(string.IsNullOrWhiteSpace(fromAddress)
            ? new MailboxAddress("Aurora College", "auroracoll-h.school@det.nsw.edu.au")
            : new MailboxAddress("Aurora College", fromAddress));

        foreach (EmailRecipient recipient in toRecipients)
        {
            _logger.Information("{id}: Adding {name} ({email}) to TO field.", id, recipient.Name, recipient.Email);
            message.To.Add(recipient.ToMailboxAddress());
        }

        if (ccRecipients.Count > 0)
            foreach (EmailRecipient recipient in ccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Name, recipient.Email);
                message.Cc.Add(recipient.ToMailboxAddress());
            }

        if (bccRecipients.Count > 0)
            foreach (EmailRecipient recipient in bccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Name, recipient.Email);
                message.Bcc.Add(recipient.ToMailboxAddress());
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;
        message.Priority = priority;

        TextPart textPartBody = new(TextFormat.Html)
        {
            Text = body
        };

        if (attachments.Count > 0 || !string.IsNullOrWhiteSpace(calendarInfo))
        {
            Multipart multipart = new("mixed")
            {
                textPartBody
            };

            if (attachments.Count > 0)
            {
                foreach (Attachment item in attachments)
                {
                    MimePart attachment = new()
                    {
                        Content = new MimeContent(item.ContentStream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    _logger.Information("{id}: Adding attachment {name}", id, item.Name);

                    multipart.Add(attachment);
                }
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

                _logger.Information("{id}: Adding calendar appointment info", id);

                multipart.Add(ical);

                message.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
            }

            message.Body = multipart;
        }
        else
        {
            message.Body = textPartBody;
        }

        if (_logOnly)
        {
            _logger.Information("SendAll: Log Only Mode");
        }
        else
        {
            _logger.Information("{id}: Sending...", id);
            Result result = await PushToServer(id, message, cancellationToken);

            if (result.IsFailure)
                return Result.Failure<MimeMessage>(result.Error);
        }

        return message;
    }

    private async Task<Result<MimeMessage>> SendAll(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        EmailRecipient fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        MessagePriority priority = MessagePriority.Normal,
        CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();

        _logger.Information("Sending email {id}", id);

        MimeMessage message = new();

        message.From.Add(fromAddress.ToMailboxAddress());
        
        foreach (EmailRecipient recipient in toRecipients)
        {
            _logger.Information("{id}: Adding {name} ({email}) to TO field.", id, recipient.Name, recipient.Email);
            message.To.Add(recipient.ToMailboxAddress());
        }

        if (ccRecipients.Count > 0)
            foreach (EmailRecipient recipient in ccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Name, recipient.Email);
                message.Cc.Add(recipient.ToMailboxAddress());
            }

        if (bccRecipients.Count > 0)
            foreach (EmailRecipient recipient in bccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Name, recipient.Email);
                message.Bcc.Add(recipient.ToMailboxAddress());
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;
        message.Priority = priority;

        TextPart textPartBody = new(TextFormat.Html)
        {
            Text = body
        };

        if (attachments.Count > 0 || !string.IsNullOrWhiteSpace(calendarInfo))
        {
            Multipart multipart = new("mixed")
            {
                textPartBody
            };

            if (attachments.Count > 0)
            {
                foreach (Attachment item in attachments)
                {
                    MimePart attachment = new()
                    {
                        Content = new MimeContent(item.ContentStream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    _logger.Information("{id}: Adding attachment {name}", id, item.Name);

                    multipart.Add(attachment);
                }
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

                _logger.Information("{id}: Adding calendar appointment info", id);

                multipart.Add(ical);

                message.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
            }

            message.Body = multipart;
        }
        else
        {
            message.Body = textPartBody;
        }

        if (_logOnly)
        {
            _logger.Information("SendAll: Log Only Mode");
        }
        else
        {
            _logger.Information("{id}: Sending...", id);
            Result result = await PushToServer(id, message, cancellationToken);

            if (result.IsFailure)
                return Result.Failure<MimeMessage>(result.Error);
        }

        return message;
    }
    
    private async Task<Result> PushToServer(Guid messageId, MimeMessage message, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
            return Result.Success();
        
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
                //.ForContext(nameof(MimeMessage), message.GetTextBody(TextFormat.Plain), true)
                .Information("{id}: Email send response: {response}", messageId, response);
        }
        catch (Exception e)
        {
            _logger
                .ForContext(nameof(Exception), e, true)
                .Error("{id}: Email send failed: {response}", messageId, e.Message);

            return Result.Failure(new("Gateway.Email.Failure", e.Message));
        }
        finally
        {
            await client.DisconnectAsync(false, cancellationToken);
        }

        return Result.Success();
    }
}
