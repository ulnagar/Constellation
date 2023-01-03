using Constellation.Application.Enums;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Infrastructure.ExternalServices.AdobeConnect
{
    public class Configuration : IAdobeConnectGatewayConfiguration, ITransientService
    {
        public Configuration(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            var settings = unitOfWork.Settings.Get().Result;

            Url = configuration["AppSettings:AdobeConnectGateway:ServerUrl"];
            Username = configuration["AppSettings:AdobeConnectGateway:Username"];
            Password = configuration["AppSettings:AdobeConnectGateway:Password"];
            BaseFolder = settings.AdobeConnectDefaultFolder;
            TemplateSco = "";

            Groups = Enum.GetValues(typeof(AdobeConnectGroup))
                .Cast<AdobeConnectGroup>()
                .ToDictionary(t => t.ToString(), t => ((int)t).ToString());
        }

        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseFolder { get; set; }
        public IDictionary<string, string> Groups { get; set; }
        public string TemplateSco { get; set; }
    }
}
