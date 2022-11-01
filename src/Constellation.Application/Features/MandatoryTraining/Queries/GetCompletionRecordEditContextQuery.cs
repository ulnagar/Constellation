namespace Constellation.Application.Features.MandatoryTraining.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record GetCompletionRecordEditContextQuery : IRequest<CompletionRecordEditContextDto>
{
    public Guid Id { get; set; }
}

public class GetCompletionRecordEditContextQueryHandler : IRequestHandler<GetCompletionRecordEditContextQuery, CompletionRecordEditContextDto>
{
	private readonly IAppDbContext _context;
	private readonly IMapper _mapper;

	public GetCompletionRecordEditContextQueryHandler(IAppDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<CompletionRecordEditContextDto> Handle(GetCompletionRecordEditContextQuery request, CancellationToken cancellationToken)
	{
		return await _context.MandatoryTraining.CompletionRecords
			.ProjectTo<CompletionRecordEditContextDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(record => record.Id == request.Id, cancellationToken);
	}
}
