using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetMatchingStudentAbsencesQueryHandler : IRequestHandler<GetMatchingStudentAbsencesQuery, ICollection<Absence>>
    {
        private readonly IAppDbContext _context;

        public GetMatchingStudentAbsencesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Absence>> Handle(GetMatchingStudentAbsencesQuery request, CancellationToken cancellationToken)
        {
            var existingAbsences = await _context.Absences
                .Where(absence => absence.StudentId == request.StudentId &&
                    absence.Date == request.AbsenceDate &&
                    absence.OfferingId == request.AbsenceOffering &&
                    absence.AbsenceTimeframe == request.AbsenceTimeframe)
                .ToListAsync(cancellationToken: cancellationToken);

            return existingAbsences;
        }
    }
}
