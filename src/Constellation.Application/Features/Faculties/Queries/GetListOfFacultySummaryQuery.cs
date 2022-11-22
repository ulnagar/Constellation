namespace Constellation.Application.Features.Faculties.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Faculties.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetListOfFacultySummaryQuery : IRequest<List<FacultySummaryDto>>
{
}

public class GetListOfFacultySummaryQueryHandler : IRequestHandler<GetListOfFacultySummaryQuery, List<FacultySummaryDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetListOfFacultySummaryQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<List<FacultySummaryDto>> Handle(GetListOfFacultySummaryQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => !faculty.IsDeleted)
            .ProjectTo<FacultySummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
