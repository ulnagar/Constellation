﻿namespace Constellation.Infrastructure.ExternalServices.Email;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.ValueObjects;
using Extensions;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using System.Net.Mime;
using ContentType = System.Net.Mime.ContentType;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

public class Gateway : IEmailGateway
{
    private readonly EmailGatewayConfiguration _configuration;
    private readonly ILogger _logger;

    private readonly bool _logOnly = true;

    public Gateway(IOptions<EmailGatewayConfiguration> configuration, ILogger logger)
    {
        _logger = logger.ForContext<IEmailGateway>();

        _configuration = configuration.Value;

        _logOnly = !_configuration.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initialised in log only mode");
        }
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        EmailRecipient fromRecipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            null,
            null,
            fromRecipient,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            null,
            null,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            null,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            null,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            calendarInfo,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            ccRecipients,
            bccRecipients,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
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
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        List<EmailRecipient> toRecipients,
        EmailRecipient fromRecipient,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toRecipients,
            null,
            null,
            fromRecipient,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }

    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            null,
            null,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            null,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            ccAddresses,
            null,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            ccAddresses,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            ccAddresses,
            bccAddresses,
            fromAddress,
            subject,
            body,
            null,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        return SendAll(
            toAddresses,
            ccAddresses,
            bccAddresses,
            fromAddress,
            subject,
            body,
            attachments,
            null,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        string fromAddress,
        string subject,
        string body,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            null,
            null,
            fromAddress,
            subject,
            body,
            null,
            calendarInfo,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            null,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            calendarInfo,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        string fromAddress,
        string subject,
        string body,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            ccAddresses,
            null,
            fromAddress,
            subject,
            body,
            null,
            calendarInfo,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            ccAddresses,
            null,
            fromAddress,
            subject,
            body,
            attachments,
            calendarInfo,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            ccAddresses,
            bccAddresses,
            fromAddress,
            subject,
            body,
            null,
            calendarInfo,
            cancellationToken);
    }
    public Task<MimeMessage> Send(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        return SendCalendarInvite(
            toAddresses,
            ccAddresses,
            bccAddresses,
            fromAddress,
            subject,
            body,
            attachments,
            calendarInfo,
            cancellationToken);
    }

    private async Task<MimeMessage> SendCalendarInvite(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();

        _logger.Information("Sending email {id}", id);

        MailMessage message = new();

        if (fromAddress == null)
            message.From = new MailAddress("auroracoll-h.school@det.nsw.edu.au", "Aurora College");
        else
            message.From = new MailAddress(fromAddress, "Aurora College");

        foreach (KeyValuePair<string, string> recipient in toAddresses)
        {
            _logger.Information("{id}: Adding {name} ({email}) to TO field.", id, recipient.Key, recipient.Value);
            message.To.Add(new MailAddress(recipient.Value, recipient.Key));
        }

        if (ccAddresses != null)
            foreach (KeyValuePair<string, string> recipient in ccAddresses)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Key, recipient.Value);
                message.CC.Add(new MailAddress(recipient.Value, recipient.Key));
            }

        if (bccAddresses != null)
            foreach (KeyValuePair<string, string> recipient in bccAddresses)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Key, recipient.Value);
                message.Bcc.Add(new MailAddress(recipient.Value, recipient.Key));
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;

        // Body
        AlternateView html = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
        message.AlternateViews.Add(html);

        // Calendar Invite
        ContentType contentType = new("text/calendar");

        string method = "";
        foreach (string line in calendarInfo.Split(Environment.NewLine))
        {
            if (line.StartsWith("METHOD", StringComparison.InvariantCultureIgnoreCase))
            {
                string[] details = line.Split(':');
                method = details[1];
            }
        }

        contentType.Parameters.Add("method", method);
        contentType.Parameters.Add("name", "Meeting.ics");
        AlternateView ical = AlternateView.CreateAlternateViewFromString(calendarInfo, contentType);
        message.AlternateViews.Add(ical);
        message.Headers.Add("Content-class", "urn:content-classes:calendarmessage");

        // Attachments
        if (attachments != null)
        {
            foreach (Attachment attachment in attachments)
            {
                message.Attachments.Add(attachment);
            }
        }

        if (_logOnly)
        {
            _logger.Information("SendCalendarInvite: Log Only Mode");
        } else
        {
            _logger.Information("{id}: Sending...", id);

            await Send(message, cancellationToken);
        }
        
        return new MimeMessage();
    }

    private async Task<MimeMessage> SendAll(
        List<EmailRecipient> toRecipients,
        List<EmailRecipient> ccRecipients,
        List<EmailRecipient> bccRecipients,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
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

        if (ccRecipients != null)
            foreach (EmailRecipient recipient in ccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Name, recipient.Email);
                message.Cc.Add(recipient.ToMailboxAddress());
            }

        if (bccRecipients != null)
            foreach (EmailRecipient recipient in bccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Name, recipient.Email);
                message.Bcc.Add(recipient.ToMailboxAddress());
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;

        TextPart textPartBody = new(TextFormat.Html)
        {
            Text = body
        };

        if (attachments != null || calendarInfo != null)
        {
            Multipart multipart = new("mixed")
            {
                textPartBody
            };

            if (attachments != null)
            {
                foreach (Attachment item in attachments)
                {
                    MimePart attachment = new()
                    {
                        Content = new MimeContent(item.ContentStream),
                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    _logger.Information("{id}: Adding attachment {name}", id, item.Name);

                    multipart.Add(attachment);
                }
            }

            if (calendarInfo != null)
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
            await PushToServer(message, cancellationToken);
        }

        return message;
    }

    private async Task<MimeMessage> SendAll(
    List<EmailRecipient> toRecipients,
    List<EmailRecipient> ccRecipients,
    List<EmailRecipient> bccRecipients,
    EmailRecipient fromAddress,
    string subject,
    string body,
    ICollection<Attachment> attachments,
    string calendarInfo,
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

        if (ccRecipients != null)
            foreach (EmailRecipient recipient in ccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Name, recipient.Email);
                message.Cc.Add(recipient.ToMailboxAddress());
            }

        if (bccRecipients != null)
            foreach (EmailRecipient recipient in bccRecipients)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Name, recipient.Email);
                message.Bcc.Add(recipient.ToMailboxAddress());
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;

        TextPart textPartBody = new(TextFormat.Html)
        {
            Text = body
        };

        if (attachments != null || calendarInfo != null)
        {
            Multipart multipart = new("mixed")
            {
                textPartBody
            };

            if (attachments != null)
            {
                foreach (Attachment item in attachments)
                {
                    MimePart attachment = new()
                    {
                        Content = new MimeContent(item.ContentStream),
                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    _logger.Information("{id}: Adding attachment {name}", id, item.Name);

                    multipart.Add(attachment);
                }
            }

            if (calendarInfo != null)
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
            await PushToServer(message, cancellationToken);
        }

        return message;
    }

    private async Task<MimeMessage> SendAll(
        IDictionary<string, string> toAddresses,
        IDictionary<string, string> ccAddresses,
        IDictionary<string, string> bccAddresses,
        string fromAddress,
        string subject,
        string body,
        ICollection<Attachment> attachments,
        string calendarInfo,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        _logger.Information("Sending email {id}", id);

        var message = new MimeMessage();

        if (string.IsNullOrWhiteSpace(fromAddress))
            message.From.Add(new MailboxAddress("Aurora College", "auroracoll-h.school@det.nsw.edu.au"));
        else
            message.From.Add(new MailboxAddress("Aurora College", fromAddress));

        foreach (var recipient in toAddresses)
        {
            _logger.Information("{id}: Adding {name} ({email}) to TO field.", id, recipient.Key, recipient.Value);
            message.To.Add(new MailboxAddress(recipient.Key, recipient.Value));
        }

        if (ccAddresses != null)
            foreach (var recipient in ccAddresses)
            {
                _logger.Information("{id}: Adding {name} ({email}) to CC field.", id, recipient.Key, recipient.Value);
                message.Cc.Add(new MailboxAddress(recipient.Key, recipient.Value));
            }

        if (bccAddresses != null)
            foreach (var recipient in bccAddresses)
            {
                _logger.Information("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Key, recipient.Value);
                message.Bcc.Add(new MailboxAddress(recipient.Key, recipient.Value));
            }

        _logger.Information("{id}: Setting Subject to \"{subject}\"", id, subject);
        message.Subject = subject;

        var textPartBody = new TextPart(TextFormat.Html)
        {
            Text = body
        };

        if (attachments != null || calendarInfo != null)
        {
            var multipart = new Multipart("mixed")
            {
                textPartBody
            };

            if (attachments != null)
            {
                foreach (var item in attachments)
                {
                    var attachment = new MimePart
                    {
                        Content = new MimeContent(item.ContentStream),
                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    _logger.Information("{id}: Adding attachment {name}", id, item.Name);

                    multipart.Add(attachment);
                }
            }

            if (calendarInfo != null)
            {
                var ical = new TextPart("calendar")
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
            await PushToServer(message, cancellationToken);
        }

        return message;
    }

    private async Task Send(MailMessage message, CancellationToken cancellationToken = default)
    {
        MimeMessage mailKitMessage = (MimeMessage)message;

        await PushToServer(mailKitMessage, cancellationToken);
    }

    private async Task PushToServer(MimeMessage message, CancellationToken cancellationToken = default)
    {
        if (_logOnly)
            return;

        using SmtpClient client = new();
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
            .Information("Email send response: {response}", response);

        await client.DisconnectAsync(false, cancellationToken);
    }
}
