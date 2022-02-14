using Constellation.Application.Common.CQRS.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SchoolRegisterJob : ISchoolRegisterJob, IScopedService, IHangfireJob
    {
        private readonly ISchoolRegisterGateway _schoolRegisterGateway;
        private readonly IMediator _mediator;

        public SchoolRegisterJob(ISchoolRegisterGateway schoolRegisterGateway, IMediator mediator)
        {
            _schoolRegisterGateway = schoolRegisterGateway;
            _mediator = mediator;
        }

        public async Task StartJob()
        {
            await _schoolRegisterGateway.UpdateSchoolDetails();
            await _schoolRegisterGateway.GetSchoolPrincipals();

            await _mediator.Send(new UpdateSchoolsFromMasterList());
        }
    }
}
