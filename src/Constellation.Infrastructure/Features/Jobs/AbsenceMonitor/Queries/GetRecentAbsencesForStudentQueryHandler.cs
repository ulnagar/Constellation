using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetRecentAbsencesForStudentQueryHandler : IRequestHandler<GetRecentAbsencesForStudentQuery, ICollection<Absence>>
    {
        private readonly IAppDbContext _context;

        public GetRecentAbsencesForStudentQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Absence>> Handle(GetRecentAbsencesForStudentQuery request, CancellationToken cancellationToken)
        {
            var recentAbsences = await _context.Absences
                .Include(absence => absence.Student)
                .Include(absence => absence.Offering)
                .Where(absence => absence.StudentId == request.StudentId &&
                    absence.Type == request.AbsenceType &&
                    !(absence.Responses.Any(response =>
                        response.Type == AbsenceResponse.Student && response.VerificationStatus == AbsenceResponse.Verified ||
                        response.Type == AbsenceResponse.Parent || response.Type == AbsenceResponse.Coordinator) ||
                    absence.ExternallyExplained) &&
                    absence.DateScanned.Date == DateTime.Today)
                .ToListAsync(cancellationToken);

            return recentAbsences;
        }
    }
}
