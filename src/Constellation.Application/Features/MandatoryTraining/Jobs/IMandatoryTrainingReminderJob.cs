namespace Constellation.Application.Features.MandatoryTraining.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IMandatoryTrainingReminderJob : IHangfireJob
{
}

public class MandatoryTrainingReminderJob : IMandatoryTrainingReminderJob
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public MandatoryTrainingReminderJob(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        // Get the list of overdue completions
        // Ignore the entries that have exemptions

        // Sort by teacher

        // Raise notification
    }
}