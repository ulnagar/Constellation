using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record UpdateTrainingModuleCommand(
    Guid Id,
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url,
    bool CanMarkNotRequired
    ) : IRequest
{ }

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
        entity.CanMarkNotRequired = request.CanMarkNotRequired;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
