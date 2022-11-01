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

public record GetCompletionRecordDetailsQuery : IRequest<CompletionRecordDto>
{
    public Guid Id { get; set; }
}

public class GetCompletionRecordDetailsQueryHandler : IRequestHandler<GetCompletionRecordDetailsQuery, CompletionRecordDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetCompletionRecordDetailsQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompletionRecordDto> Handle(GetCompletionRecordDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _context.MandatoryTraining.CompletionRecords
            .ProjectTo<CompletionRecordDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(record => record.Id == request.Id, cancellationToken);
    }
}
