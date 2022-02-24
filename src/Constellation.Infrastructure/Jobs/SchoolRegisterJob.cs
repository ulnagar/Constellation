using Constellation.Application.Common.CQRS.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using System;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SchoolRegisterJob : ISchoolRegisterJob, IScopedService, IHangfireJob
    {
        private readonly ISchoolRegisterGateway _schoolRegisterGateway;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public SchoolRegisterJob(ISchoolRegisterGateway schoolRegisterGateway, IMediator mediator,
            IUnitOfWork unitOfWork)
        {
            _schoolRegisterGateway = schoolRegisterGateway;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _unitOfWork.JobActivations.GetForJob(nameof(ISchoolRegisterJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    Console.WriteLine("Stopped due to job being set inactive.");
                    return;
                }
            }

            await _schoolRegisterGateway.UpdateSchoolDetails();

            // Do not update Principal data as this might overwrite custom data updates
            //await _schoolRegisterGateway.GetSchoolPrincipals();

            await _mediator.Send(new UpdateSchoolsFromMasterList());
        }
    }
}
