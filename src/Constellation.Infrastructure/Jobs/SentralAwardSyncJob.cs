using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Commands;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralAwardSyncJob : ISentralAwardSyncJob, IScopedService, IHangfireJob
    {
        private readonly ILogger<ISentralAwardSyncJob> _logger;
        private readonly ISentralGateway _gateway;
        private readonly IMediator _mediator;

        public SentralAwardSyncJob(ILogger<ISentralAwardSyncJob> logger, ISentralGateway gateway, IMediator mediator)
        {
            _logger = logger;
            _gateway = gateway;
            _mediator = mediator;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            _logger.LogInformation("{id}: Starting Sentral Awards Scan.", jobId);

            var details = await _gateway.GetAwardsReport();

            // Process individual students
            // Tally awards
            // Calculate expected award levels
            // Highlight discrepancies

            foreach (var group in details.GroupBy(detail => detail.StudentId))
            {
                var student = await _mediator.Send(new GetStudentWithAwardQuery { StudentId = group.Key }, token);

                _logger.LogInformation("{id}: Scanning {studentName} ({studentGrade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

                foreach (var item in group)
                {
                    if (!student.Awards.Any(award => award.Type == item.AwardType && award.AwardedOn == item.AwardCreated))
                    {
                        _logger.LogInformation("{id}: Found new {type} on {date}", jobId, item.AwardType, item.AwardCreated.ToShortDateString());

                        await _mediator.Send(new CreateStudentAwardCommand
                        {
                            StudentId = student.StudentId,
                            Category = item.AwardCategory,
                            Type = item.AwardType,
                            AwardedOn = item.AwardCreated
                        }, token);
                    }
                }
            }
        }
    }
}
