namespace Constellation.Application.Features.MandatoryTraining.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetListOfCompletionRecordsQuery : IRequest<List<CompletionRecordDto>>
{
}

public class GetListOfCompletionRecordsQueryHandler : IRequestHandler<GetListOfCompletionRecordsQuery, List<CompletionRecordDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetListOfCompletionRecordsQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CompletionRecordDto>> Handle(GetListOfCompletionRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _context.MandatoryTraining.CompletionRecords
            .ProjectTo<CompletionRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return records;
    }
}
