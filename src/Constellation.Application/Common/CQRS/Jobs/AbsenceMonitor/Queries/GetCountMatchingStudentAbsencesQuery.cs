using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries
{
    public class GetCountMatchingStudentAbsencesQuery : IRequest<int>
    {
        public string StudentId { get; set; }
        public DateTime AbsenceDate { get; set; }
        public int AbsenceOffering { get; set; }
        public string AbsenceTimeframe { get; set; }
    }

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
