using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    public class EmailGateway : IEmailGateway, IScopedService
    {
        private IEmailGatewayConfiguration _configuration;
        private readonly ILogger<IEmailGateway> _logger;

        public EmailGateway(IEmailGatewayConfiguration configuration, ILogger<IEmailGateway> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body)
        {
            return SendAll(toAddresses, null, null, fromAddress, subject, body, null, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments)
        {
            return SendAll(toAddresses, null, null, fromAddress, subject, body, attachments, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body)
        {
            return SendAll(toAddresses, ccAddresses, null, fromAddress, subject, body, null, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments)
        {
            return SendAll(toAddresses, ccAddresses, null, fromAddress, subject, body, attachments, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body)
        {
            return SendAll(toAddresses, ccAddresses, bccAddresses, fromAddress, subject, body, null, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments)
        {
            return SendAll(toAddresses, ccAddresses, bccAddresses, fromAddress, subject, body, attachments, null);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, null, null, fromAddress, subject, body, null, calendarInfo);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, null, null, fromAddress, subject, body, attachments, calendarInfo);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, ccAddresses, null, fromAddress, subject, body, null, calendarInfo);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, ccAddresses, null, fromAddress, subject, body, attachments, calendarInfo);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, ccAddresses, bccAddresses, fromAddress, subject, body, null, calendarInfo);
        }
        public Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo)
        {
            return SendCalendarInvite(toAddresses, ccAddresses, bccAddresses, fromAddress, subject, body, attachments, calendarInfo);
        }

        private async Task<MimeMessage> SendCalendarInvite(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo)
        {
            var id = Guid.NewGuid();

            _logger.LogInformation("Sending email {id}", id);

            var message = new MailMessage();

            if (fromAddress == null)
                message.From = new MailAddress("auroracoll-h.school@det.nsw.edu.au", "Aurora College");
            else
                message.From = new MailAddress(fromAddress, "Aurora College");

            foreach (var recipient in toAddresses)
            {
                _logger.LogInformation("{id}: Adding {name} ({email}) to TO field.", id, recipient.Key, recipient.Value);
                message.To.Add(new MailAddress(recipient.Value, recipient.Key));
            }

            if (ccAddresses != null)
                foreach (var recipient in ccAddresses)
                {
                    _logger.LogInformation("{id}: Adding {name} ({email}) to CC field.", id, recipient.Key, recipient.Value);
                    message.CC.Add(new MailAddress(recipient.Value, recipient.Key));
                }

            if (bccAddresses != null)
                foreach (var recipient in bccAddresses)
                {
                    _logger.LogInformation("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Key, recipient.Value);
                    message.Bcc.Add(new MailAddress(recipient.Value, recipient.Key));
                }

            _logger.LogInformation("{id}: Setting Subject to \"{subject}\"", id, subject);
            message.Subject = subject;

            // Body
            var html = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(html);

            // Calendar Invite
            var contentType = new System.Net.Mime.ContentType("text/calendar");

            var method = "";
            foreach (var line in calendarInfo.Split(Environment.NewLine))
            {
                if (line.StartsWith("METHOD"))
                {
                    var details = line.Split(':');
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
                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }
            }

            _logger.LogInformation("{id}: Sending...", id);
            //var client = new SmtpClient
            //{
            //    Host = _configuration.Server,
            //    Port = _configuration.Port
            //};

            //client.Send(message);

            await Send(message);

            return new MimeMessage();
        }

        private async Task<MimeMessage> SendAll(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo)
        {
            var id = Guid.NewGuid();

            _logger.LogInformation("Sending email {id}", id);

            var message = new MimeMessage();

            if (fromAddress == null)
                message.From.Add(new MailboxAddress("Aurora College", "auroracoll-h.school@det.nsw.edu.au"));
            else
                message.From.Add(new MailboxAddress("Aurora College", fromAddress));

            foreach (var recipient in toAddresses)
            {
                _logger.LogInformation("{id}: Adding {name} ({email}) to TO field.", id, recipient.Key, recipient.Value);
                message.To.Add(new MailboxAddress(recipient.Key, recipient.Value));
            }

            if (ccAddresses != null)
                foreach (var recipient in ccAddresses)
                {
                    _logger.LogInformation("{id}: Adding {name} ({email}) to CC field.", id, recipient.Key, recipient.Value);
                    message.Cc.Add(new MailboxAddress(recipient.Key, recipient.Value));
                }

            if (bccAddresses != null)
                foreach (var recipient in bccAddresses)
                {
                    _logger.LogInformation("{id}: Adding {name} ({email}) to BCC field.", id, recipient.Key, recipient.Value);
                    message.Bcc.Add(new MailboxAddress(recipient.Key, recipient.Value));
                }

            _logger.LogInformation("{id}: Setting Subject to \"{subject}\"", id, subject);
            message.Subject = subject;
            
            var textPartBody = new TextPart(TextFormat.Html)
            {
                Text = body
            };

            if (attachments != null || calendarInfo != null)
            {
                var multipart = new Multipart("mixed");
                multipart.Add(textPartBody);

                if (attachments != null)
                {
                    foreach (var item in attachments)
                    {
                        var attachment = new MimePart
                        {
                            Content = new MimeContent(item.ContentStream, ContentEncoding.Default),
                            ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = item.Name
                        };

                        _logger.LogInformation("{id}: Adding attachment {name}", id, item.Name);

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

                    _logger.LogInformation("{id}: Adding calendar appointment info", id);

                    multipart.Add(ical);

                    message.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
                }

                message.Body = multipart;
            } else
            {
                message.Body = textPartBody;
            }

            _logger.LogInformation("{id}: Sending...", id);
            await PushToServer(message);

            return message;
        }

        private async Task Send(MailMessage message)
        {
            var mailKitMessage = (MimeMessage)message;

            await PushToServer(mailKitMessage);
        }

        private async Task PushToServer(MimeMessage message)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_configuration.Server, _configuration.Port, SecureSocketOptions.StartTlsWhenAvailable);
            if (!string.IsNullOrWhiteSpace(_configuration.Username))
                await client.AuthenticateAsync(_configuration.Username, _configuration.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(false);
        }
    }
}
