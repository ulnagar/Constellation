using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
                .Where(student => student.StudentId == request.StudentId && !student.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (student == null)
                return record;

            record.StudentId = request.StudentId;
            record.StudentName = student.DisplayName;
            record.StudentGrade = student.CurrentGrade.AsName();
            record.StudentSchool = student.School.Name;

            var sessions = await _context.Sessions
                .Include(session => session.Period)
                .Include(session => session.Teacher)
                .Include(session => session.Offering)
                .Where(session => session.Offering.Enrolments.Any(enrolment => enrolment.StudentId == request.StudentId && !enrolment.IsDeleted) && !session.IsDeleted)
                .ToListAsync(cancellationToken);

            if (sessions == null)
                return record;

            var periods = await _context.Periods.ToListAsync(cancellationToken);

            // Find relevantPeriods
            // Extrapolate relevantTimetables
            // Collect all Period information for relevantTimetables
            // Add class/session data for each Period

            var relevantTimetables = sessions.Select(session => session.Period.Timetable).Distinct().ToList();

            var relevantPeriods = periods.Where(period => relevantTimetables.Contains(period.Timetable)).ToList();

            foreach (var period in relevantPeriods)
            {
                if (period.Type == "Other")
                    continue;

                var entry = new StudentTimetableDataDto.TimetableData
                {
                    Day = period.Day,
                    StartTime = period.StartTime,
                    EndTime = period.EndTime,
                    TimetableName = period.Timetable,
                    Name = period.Name,
                    Period = period.Period,
                    Type = period.Type
                };

                if (sessions.Any(session => session.PeriodId == period.Id))
                {
                    var relevantSession = sessions.FirstOrDefault(session => session.PeriodId == period.Id);
                    entry.ClassName = relevantSession.Offering.Name;
                    entry.ClassTeacher = relevantSession.Teacher.DisplayName;
                }

                record.Timetables.Add(entry);
            }

            return record;
        }
    }
}
