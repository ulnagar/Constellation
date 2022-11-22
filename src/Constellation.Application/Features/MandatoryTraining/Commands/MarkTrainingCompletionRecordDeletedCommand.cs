namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record MarkTrainingCompletionRecordDeletedCommand(Guid RecordId) : IRequest { }

public class MarkTrainingCompletionRecordDeletedCommandHandler : IRequestHandler<MarkTrainingCompletionRecordDeletedCommand>
{
    private readonly IAppDbContext _context;

    public MarkTrainingCompletionRecordDeletedCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(MarkTrainingCompletionRecordDeletedCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.MandatoryTraining.CompletionRecords
            .FirstOrDefaultAsync(record => record.Id == request.RecordId, cancellationToken);

        if (record is not null) 
        {
            record.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
