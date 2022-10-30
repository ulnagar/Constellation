using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Queries;

public record GetListOfModuleSummaryQuery : IRequest<List<ModuleSummaryDto>>
{
}

public class GetListOfModuleSummaryQueryHandler : IRequestHandler<GetListOfModuleSummaryQuery, List<ModuleSummaryDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetListOfModuleSummaryQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ModuleSummaryDto>> Handle(GetListOfModuleSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _context.MandatoryTraining.Modules
            .ProjectTo<ModuleSummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
