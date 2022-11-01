using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record CreateTrainingModuleCommand : IRequest
{
    public string Name { get; init; }
    public TrainingModuleExpiryFrequency Expiry { get; init; }
    public string Url { get; init; }
    public string CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class CreateTrainingModuleCommandHandler : IRequestHandler<CreateTrainingModuleCommand>
{
    private readonly IAppDbContext _context;

    public CreateTrainingModuleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CreateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var entity = new TrainingModule
        {
            Name = request.Name,
            Expiry = request.Expiry,
            Url = request.Url,
            CreatedBy = request.CreatedBy,
            CreatedAt = request.CreatedAt
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
