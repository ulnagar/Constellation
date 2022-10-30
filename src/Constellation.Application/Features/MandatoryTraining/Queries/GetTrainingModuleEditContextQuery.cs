using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Queries;

public record GetTrainingModuleEditContextQuery : IRequest<ModuleEditContextDto>
{
    public Guid Id { get; init; }
}

public class GetTrainingModuleEditContextQueryHandler : IRequestHandler<GetTrainingModuleEditContextQuery, ModuleEditContextDto>
{
	private readonly IAppDbContext _context;
	private readonly IMapper _mapper;

	public GetTrainingModuleEditContextQueryHandler(IAppDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<ModuleEditContextDto> Handle(GetTrainingModuleEditContextQuery request, CancellationToken cancellationToken)
	{
		return await _context.MandatoryTraining.Modules
			.ProjectTo<ModuleEditContextDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);
	}
}
