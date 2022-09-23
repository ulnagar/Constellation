namespace Constellation.Application.Features.Awards.Queries;

using AutoMapper;
using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAwardSummaryForStudentQuery : IRequest<StudentAwardSummary>
{
    public string StudentId { get; set; }
}

public class StudentAwardSummary
{
    public int Astras { get; set; }
    public int Stellars { get; set; }
    public int Galaxies { get; set; }
    public int Universals { get; set; }
    public List<StudentAwardsForList> RecentAwards { get; set; } = new();
}

public class GetAwardSummaryForStudentQueryHandler : IRequestHandler<GetAwardSummaryForStudentQuery, StudentAwardSummary>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAwardSummaryForStudentQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StudentAwardSummary> Handle(GetAwardSummaryForStudentQuery request, CancellationToken cancellationToken)
    {
        var summary = new StudentAwardSummary();

        var data = await _context.Students
            .Where(student => student.StudentId == request.StudentId)
            .SelectMany(student => student.Awards)
            .ToListAsync(cancellationToken);

        summary.Astras = data.Count(award => award.Type == "Astra Award");
        summary.Stellars = data.Count(award => award.Type == "Stellar Award");
        summary.Galaxies = data.Count(award => award.Type == "Galaxy Medal");
        summary.Universals = data.Count(award => award.Type == "Aurora Universal Achiever");

        summary.RecentAwards = _mapper.Map<List<StudentAwardsForList>>(data.OrderByDescending(award => award.AwardedOn).Take(10).ToList());

        return summary;
    }
}
