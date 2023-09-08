namespace Constellation.Infrastructure.Features.Portal.School.Timetables.Queries;

using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;

public class GetStudentTimetableDataQueryHandler : IRequestHandler<GetStudentTimetableDataQuery, StudentTimetableDataDto>
{
    private readonly AppDbContext _context;

    public GetStudentTimetableDataQueryHandler(AppDbContext context)
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

        var offeringIds = await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                enrolment.StudentId == student.StudentId &&
                enrolment.IsDeleted)
            .Select(enrolment => enrolment.OfferingId)
            .ToListAsync(cancellationToken);

        var offerings = await _context
            .Set<Offering>()
            .Where(offering => offeringIds.Contains(offering.Id))
            .ToListAsync(cancellationToken);

        var periodIds = offerings
            .SelectMany(offering => offering.Sessions)
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .ToList();

        var periods = await _context
            .Set<TimetablePeriod>()
            .Where(period => periodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);

        var relevantTimetables = periods
            .Select(period => period.Timetable)
            .Distinct()
            .ToList();

        var relevantPeriods = await _context
            .Set<TimetablePeriod>()
            .Where(period => relevantTimetables.Contains(period.Timetable))
            .ToListAsync(cancellationToken);

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


            if (periodIds.Contains(period.Id))
            {
                var offering = offerings
                    .Where(offering =>
                        offering.Sessions.Any(session =>
                            session.PeriodId == period.Id))
                    .FirstOrDefault();

                if (offering is null)
                    continue;

                entry.ClassName = offering.Name;

                List<TeacherAssignment> assignments = offering
                    .Teachers
                    .Where(assignment =>
                        assignment.Type == AssignmentType.ClassroomTeacher &&
                        !assignment.IsDeleted)
                    .ToList();

                foreach (TeacherAssignment assignment in assignments)
                {
                    Staff teacher = await _context
                        .Set<Staff>()
                        .Where(teacher => teacher.StaffId == assignment.StaffId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (teacher is null)
                        continue;

                    entry.ClassTeacher = teacher.DisplayName;
                }
            }

            record.Timetables.Add(entry);
        }

        return record;
    }
}
