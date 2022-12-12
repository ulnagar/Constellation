namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public record CreateTrainingModuleCommand(
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url,
    bool CanMarkNotRequired
    ) : IRequest
{ }

public class CreateTrainingModuleCommandHandler : IRequestHandler<CreateTrainingModuleCommand>
{
    private readonly IAppDbContext _context;

    public CreateTrainingModuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CreateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var entity = new TrainingModule(Guid.NewGuid())
        {
            Name = request.Name,
            Expiry = request.Expiry,
            Url = request.Url,
            CanMarkNotRequired = request.CanMarkNotRequired,
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
