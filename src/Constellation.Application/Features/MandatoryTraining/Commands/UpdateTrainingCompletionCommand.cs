using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record UpdateTrainingCompletionCommand : IRequest
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public Guid TrainingModuleId { get; set; }
    public DateTime CompletedDate { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

}

public class UpdateTrainingCompletionCommandHandler : IRequestHandler<UpdateTrainingCompletionCommand>
{
    private readonly IAppDbContext _context;

    public UpdateTrainingCompletionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MandatoryTraining.CompletionRecords
            .FirstOrDefaultAsync(record => record.Id == request.Id, cancellationToken);

        entity.StaffId = request.StaffId;
        entity.TrainingModuleId = request.TrainingModuleId;
        entity.CompletedDate = request.CompletedDate;
        entity.ModifiedBy = request.ModifiedBy;
        entity.ModifiedAt = request.ModifiedAt;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
