using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Infrastructure.DependencyInjection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SchoolRegisterJob : ISchoolRegisterJob, IScopedService
    {
        private readonly ISchoolRegisterGateway _schoolRegisterGateway;

        public SchoolRegisterJob(ISchoolRegisterGateway schoolRegisterGateway)
        {
            _schoolRegisterGateway = schoolRegisterGateway;
        }

        public async Task StartJob()
        {
            await _schoolRegisterGateway.UpdateSchoolDetails();
            await _schoolRegisterGateway.GetSchoolPrincipals();
        }
    }
}
