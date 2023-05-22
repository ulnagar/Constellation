using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Absences.Models;
using Constellation.Application.Features.Portal.School.Absences.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Absences;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Queries
{
    public class GetUnProcessedAbsencesFromSchoolQueryHandler : IRequestHandler<GetUnProcessedAbsencesFromSchoolQuery, ICollection<AbsenceForPortalList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetUnProcessedAbsencesFromSchoolQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<AbsenceForPortalList>> Handle(GetUnProcessedAbsencesFromSchoolQuery request, CancellationToken cancellationToken)
        {
            //var schoolAbsencesForThisYear = _context.Absences
            //    .Where(absence => absence.Student.SchoolCode == request.SchoolCode && !absence.Student.IsDeleted && absence.Date.Year == DateTime.Today.Year).ToList();

            //var unexplainedAbsencesForThisYear = schoolAbsencesForThisYear
            //    .Where(absence =>
            //        !absence.ExternallyExplained &&
            //        ((absence.Type == Absence.Partial && absence.Responses.Count(response => response.VerificationStatus != AbsenceResponse.Pending) == 0) ||
            //        (absence.Type == Absence.Whole && absence.Responses.Count == 0))).ToList();

            //return _mapper.Map<ICollection<AbsenceForPortalList>>(unexplainedAbsencesForThisYear);

            return await _context.Absences
                .Where(absence => absence.Student.SchoolCode == request.SchoolCode && !absence.Student.IsDeleted && absence.Date.Year == DateTime.Today.Year)
                .Where(absence =>
                    !absence.ExternallyExplained &&
                    ((absence.Type == Absence.Partial && absence.Responses.Count(response => response.VerificationStatus != AbsenceResponse.Pending) == 0) ||
                    (absence.Type == Absence.Whole && absence.Responses.Count == 0)))
                .ProjectTo<AbsenceForPortalList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
