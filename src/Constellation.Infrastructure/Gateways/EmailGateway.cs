using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using MailKit.Security;
using MimeKit;
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

        public async Task Send(MailMessage message)
        {
            var mailKitMessage = (MimeMessage)message;

            await Send(mailKitMessage);
        }

        public async Task Send(MimeMessage message)
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
