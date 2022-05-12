using Constellation.Application.Features.Portal.School.Timetables.Models;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Timetables.Queries
{
    public class GetStudentTimetableDataQueryHandler : IRequestHandler<GetStudentTimetableDataQuery, StudentTimetableDataDto>
    {
        private readonly IAppDbContext _context;

        public GetStudentTimetableDataQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<StudentTimetableDataDto> Handle(GetStudentTimetableDataQuery request, CancellationToken cancellationToken)
        {
            var record = new StudentTimetableDataDto();

            var student = await _context.Students
                .Include(student => student.School)
                .Where(student => student.StudentId == request.StudentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (student == null)
                return record;

            var sessions = await _context.Sessions
                .Include(session => session.Period)
                .Include(session => session.Teacher)
                .Where(session => session.Offering.Enrolments.Any(enrolment => enrolment.StudentId == request.StudentId && !enrolment.IsDeleted))
                .ToListAsync(cancellationToken);

            if (sessions == null)
                return record;

            var periods = await _context.Periods.ToListAsync(cancellationToken);

            // Find relevantPeriods
            // Extrapolate relevantTimetables
            // Collect all Period information for relevantTimetables
            // Add class/session data for each Period

            return record;
        }
    }
}
