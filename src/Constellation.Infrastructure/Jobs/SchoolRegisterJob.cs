using Constellation.Application.Features.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using System;
using System.Threading;
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

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            await _schoolRegisterGateway.UpdateSchoolDetails();

            // Do not update Principal data as this might overwrite custom data updates
            //await _schoolRegisterGateway.GetSchoolPrincipals();

            await _mediator.Send(new UpdateSchoolsFromMasterList());
        }
    }
}
