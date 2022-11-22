using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.MandatoryTraining;
namespace Constellation.Application.Features.MandatoryTraining.Commands;

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record RetireTrainingModuleCommand : IRequest
{
    public Guid Id { get; set; }
	public string DeletedBy { get; set; }
	public DateTime DeletedAt { get; set; }
}

public class RetireTrainingModuleCommandHandler : IRequestHandler<RetireTrainingModuleCommand>
{
	private readonly IAppDbContext _context;

	public RetireTrainingModuleCommandHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<Unit> Handle(RetireTrainingModuleCommand request, CancellationToken cancellationToken)
	{
        var entity = await _context.MandatoryTraining.Modules
            .FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);

		entity.IsDeleted = true;

		await _context.SaveChangesAsync(cancellationToken);

		return Unit.Value;
	}
}
