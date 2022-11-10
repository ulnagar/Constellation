namespace Constellation.Application.Features.MandatoryTraining.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetListOfCompletionRecordsQuery : IRequest<List<CompletionRecordDto>> 
{ 
    public string StaffId { get; init; }
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
        var records = _context.MandatoryTraining.CompletionRecords
            .AsNoTracking();

        // If a StaffId was specified, then filter the query
        if (!string.IsNullOrWhiteSpace(request.StaffId))
        {
            records = records.Where(record => record.StaffId == request.StaffId);
        }

        return await records.ProjectTo<CompletionRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
