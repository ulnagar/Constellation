using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Constellation.Infrastructure.ExternalServices.Sentral
{
    public class Configuration : ISentralGatewayConfiguration, ITransientService
    {
        public Configuration(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            var settings = unitOfWork.Settings.Get().Result;

            Username = configuration["AppSettings:SentralGateway:Username"];
            Password = configuration["AppSettings:SentralGateway:Password"];
            Server = configuration["AppSettings:SentralGateway:ServerUrl"];
            ContactPreference = settings.SentralContactPreference;

            XPaths = new Dictionary<string, string>
            {
                { "MothersHomePhone", "//*[@id='expander-content-2']/table/tr/td[1]/table[2]/tr[1]/td" },
                { "MothersWorkPhone", "//*[@id='expander-content-2']/table/tr/td[1]/table[2]/tr[2]/td" },
                { "MothersMobilePhone", "//*[@id='expander-content-2']/table/tr/td[1]/table[2]/tr[3]/td" },
                { "MothersEmail", "//*[@id='expander-content-2']/table/tr/td[1]/table[2]/tr[4]/td" },
                { "FathersHomePhone", "//*[@id='expander-content-2']/table/tr/td[2]/table[2]/tr[1]/td" },
                { "FathersWorkPhone", "//*[@id='expander-content-2']/table/tr/td[2]/table[2]/tr[2]/td" },
                { "FathersMobilePhone", "//*[@id='expander-content-2']/table/tr/td[2]/table[2]/tr[3]/td" },
                { "FathersEmail", "//*[@id='expander-content-2']/table/tr/td[2]/table[2]/tr[4]/td" },
                { "FamilyName", "//*[@id='expander-content-1']/table/tr/td[1]/table/tr[2]/td" },
                { "AbsenceTable", "//*[@id='layout-2col-content']/div/div[3]/div[2]/table/tbody" },
                { "StudentTable", "//*[@id='layout-2col-content']/div/div[1]/div/div[2]/table/tbody" },
                { "PartialAbsenceTable", "//*[@id='student-absences-list']/table/tbody" },
                { "CalendarTable", "//*[@id='layout-2col-content']/div/div/div[2]/div/table[1]" },
                { "TermCalendarTable", "//*[@id='layout-2col-content']/div/div/div[2]/div/div/table" },
                //{ "WellbeingStudentAwardsList", "/html/body/div[7]/div/div/div[2]/div/div/div/div[2]/table/tbody" },
                { "WellbeingStudentAwardsList", "//*[@id='layout-2col-content']/div/div/div[2]/table/tbody" }
            };
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string ContactPreference { get; set; }
        public IDictionary<string, string> XPaths { get; set; }
    }
}
