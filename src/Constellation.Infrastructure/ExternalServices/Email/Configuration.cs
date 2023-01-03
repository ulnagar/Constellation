using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace Constellation.Infrastructure.ExternalServices.Email
{
    public class Configuration : IEmailGatewayConfiguration, ITransientService
    {
        public Configuration(IConfiguration configuration)
        {
            Server = configuration["AppSettings:EmailGateway:Server"];
            Port = Convert.ToInt32(configuration["AppSettings:EmailGateway:Port"]);
            Username = configuration["AppSettings:EmailGateway:Username"];
            Password = configuration["AppSettings:EmailGateway:Password"];
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
