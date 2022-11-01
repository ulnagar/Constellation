namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record ReinstateTrainingModuleCommand : IRequest
{
    public Guid Id { get; set; }
}

public class ReinstateTrainingModuleCommandHandler : IRequestHandler<ReinstateTrainingModuleCommand>
{
	private readonly IAppDbContext _context;

	public ReinstateTrainingModuleCommandHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<Unit> Handle(ReinstateTrainingModuleCommand request, CancellationToken cancellationToken)
	{
        var entity = await _context.MandatoryTraining.Modules
            .FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);

        entity.DeletedBy = null;
		entity.DeletedAt = new DateTime();

		await _context.SaveChangesAsync(cancellationToken);

		return Unit.Value;
	}
}
