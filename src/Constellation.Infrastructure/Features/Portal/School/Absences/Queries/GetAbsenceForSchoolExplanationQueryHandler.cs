using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Absences.Models;
using Constellation.Application.Features.Portal.School.Absences.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Queries
{
    public class GetAbsenceForSchoolExplanationQueryHandler : IRequestHandler<GetAbsenceForSchoolExplanationQuery, WholeAbsenceForSchoolExplanation>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAbsenceForSchoolExplanationQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<WholeAbsenceForSchoolExplanation> Handle(GetAbsenceForSchoolExplanationQuery request, CancellationToken cancellationToken)
        {
            return await _context.Absences
                .Where(absence => absence.Id == request.Id)
                .ProjectTo<WholeAbsenceForSchoolExplanation>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
