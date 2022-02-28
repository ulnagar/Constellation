using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    public class EmailGateway : IEmailGateway, IScopedService
    {
        private IEmailGatewayConfiguration _configuration;

        public EmailGateway(IEmailGatewayConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments)
        {
            var message = new MimeMessage();

            if (fromAddress == null)
                message.From.Add(new MailboxAddress("Aurora College", "auroracoll-h.school@det.nsw.edu.au"));
            else
                message.From.Add(new MailboxAddress("Aurora College", fromAddress));

            foreach (var recipient in toAddresses)
                message.To.Add(new MailboxAddress(recipient.Key, recipient.Value));

            if (ccAddresses != null)
                foreach (var recipient in ccAddresses)
                    message.Cc.Add(new MailboxAddress(recipient.Key, recipient.Value));

            if (bccAddresses != null)
                foreach (var recipient in bccAddresses)
                    message.Bcc.Add(new MailboxAddress(recipient.Key, recipient.Value));

            message.Subject = subject;
            
            var textPartBody = new TextPart(TextFormat.Html)
            {
                Text = body
            };

            if (attachments != null)
            {
                var multipart = new Multipart("mixed");
                multipart.Add(textPartBody);

                foreach (var item in attachments)
                {
                    var attachment = new MimePart
                    {
                        Content = new MimeContent(item.ContentStream, ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = item.Name
                    };

                    multipart.Add(attachment);
                }

                message.Body = multipart;
            } else
            {
                message.Body = textPartBody;
            }

            await PushToServer(message);

            return message;
        }

        public async Task Send(MailMessage message)
        {
            var mailKitMessage = (MimeMessage)message;

            await PushToServer(mailKitMessage);
        }

        private async Task PushToServer(MimeMessage message)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(_configuration.Server, _configuration.Port, SecureSocketOptions.StartTlsWhenAvailable);
            if (!string.IsNullOrWhiteSpace(_configuration.Username))
                await client.AuthenticateAsync(_configuration.Username, _configuration.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(false);
        }
    }
}
