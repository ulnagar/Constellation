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

public record GetModuleDetailsQuery : IRequest<ModuleDetailsDto>
{
    public Guid Id { get; init; }
}

public class GetModuleDetailsQueryHandler : IRequestHandler<GetModuleDetailsQuery, ModuleDetailsDto>
{
	private readonly IAppDbContext _context;
	private readonly IMapper _mapper;

	public GetModuleDetailsQueryHandler(IAppDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<ModuleDetailsDto> Handle(GetModuleDetailsQuery request, CancellationToken cancellationToken)
	{
		return await _context.MandatoryTraining.Modules
			.ProjectTo<ModuleDetailsDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(module => module.Id == request.Id, cancellationToken);
	}
}
