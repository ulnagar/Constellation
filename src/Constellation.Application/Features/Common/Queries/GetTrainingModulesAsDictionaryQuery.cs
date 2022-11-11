namespace Constellation.Application.Features.Common.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetTrainingModulesAsDictionaryQuery : IRequest<Dictionary<Guid, string>>
{
}

public class GetTrainingModulesAsDictionaryQueryHandler : IRequestHandler<GetTrainingModulesAsDictionaryQuery, Dictionary<Guid, string>>
{
	private readonly IAppDbContext _context;

	public GetTrainingModulesAsDictionaryQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<Dictionary<Guid, string>> Handle(GetTrainingModulesAsDictionaryQuery request, CancellationToken cancellationToken)
	{
		return await _context.MandatoryTraining.Modules
			.Where(module => string.IsNullOrWhiteSpace(module.DeletedBy))
			.OrderBy(module => module.Name)
			.ToDictionaryAsync(module => module.Id, module => module.Name, cancellationToken);
	}
}
