using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetCountMatchingStudentAbsencesQueryHandler : IRequestHandler<GetCountMatchingStudentAbsencesQuery, int>
    {
        private readonly IAppDbContext _context;

        public GetCountMatchingStudentAbsencesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(GetCountMatchingStudentAbsencesQuery request, CancellationToken cancellationToken)
        {
            var existingAbsences = await _context.Absences
                .CountAsync(absence => absence.StudentId == request.StudentId &&
                    absence.Date == request.AbsenceDate &&
                    absence.OfferingId == request.AbsenceOffering &&
                    absence.AbsenceTimeframe == request.AbsenceTimeframe, cancellationToken: cancellationToken);

            return existingAbsences;
        }
    }
}
