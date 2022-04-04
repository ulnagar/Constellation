using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries
{
    public class GetUnexplainedAbsencesForDigestQuery : IRequest<ICollection<Absence>>
    {
        public string StudentId { get; set; }
        public string Type { get; set; }
        public int AgeInWeeks { get; set; }
    }

    public class GetUnexplainedAbsencesForDigestQueryHandler : IRequestHandler<GetUnexplainedAbsencesForDigestQuery, ICollection<Absence>>
    {
        private readonly IAppDbContext _context;

        public GetUnexplainedAbsencesForDigestQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Absence>> Handle(GetUnexplainedAbsencesForDigestQuery request, CancellationToken cancellationToken)
        {
            var coordinatorDigestAbsences = await _context.Absences
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Offering)
                .Where(absence => absence.StudentId == request.StudentId &&
                    absence.Type == request.Type &&
                    (absence.Responses.Any(response =>
                        (response.Type == AbsenceResponse.Student && response.VerificationStatus == AbsenceResponse.Verified) ||
                        (response.Type == AbsenceResponse.Parent || response.Type == AbsenceResponse.Coordinator)) ||
                    absence.ExternallyExplained) &&
                    absence.DateScanned >= DateTime.Today.AddDays(request.AgeInWeeks * -7) &&
                    absence.DateScanned <= DateTime.Today.AddDays(((request.AgeInWeeks -1) * -7) - 1))
                .ToListAsync(cancellationToken);

            return coordinatorDigestAbsences;
        }
    }
}
