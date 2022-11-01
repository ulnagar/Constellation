using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record UpdateTrainingModuleCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public TrainingModuleExpiryFrequency Expiry { get; init; }
    public string Url { get; init; }
    public string ModifiedBy { get; init; }
    public DateTime ModifiedAt { get; init; }
}

public class UpdateTrainingModuleCommandHandler : IRequestHandler<UpdateTrainingModuleCommand>
{
    private readonly IAppDbContext _context;

    public UpdateTrainingModuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MandatoryTraining.Modules
            .FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);

        entity.Name = request.Name;
        entity.Expiry = request.Expiry;
        entity.Url = request.Url;
        entity.ModifiedBy = request.ModifiedBy;
        entity.ModifiedAt = request.ModifiedAt;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
