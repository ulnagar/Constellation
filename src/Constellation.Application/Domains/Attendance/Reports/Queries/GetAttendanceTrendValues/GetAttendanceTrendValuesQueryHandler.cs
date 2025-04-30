namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceTrendValues;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceTrendValuesQueryHandler
: IQueryHandler<GetAttendanceTrendValuesQuery, List<AttendanceTrend>>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public GetAttendanceTrendValuesQueryHandler(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<Result<List<AttendanceTrend>>> Handle(GetAttendanceTrendValuesQuery request, CancellationToken cancellationToken)
    {
        List<AttendanceTrend> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<AttendanceValue> values = await _attendanceRepository.GetAllRecent(cancellationToken);

        IEnumerable<IGrouping<StudentId, AttendanceValue>> groupedValues = values.OrderByDescending(value => value.EndDate).GroupBy(value => value.StudentId);

        foreach (IGrouping<StudentId, AttendanceValue> studentEntries in groupedValues)
        {
            Student student = students.FirstOrDefault(entry => entry.Id == studentEntries.Key);

            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null) 
                continue;

            string period = studentEntries.First().PeriodLabel;

            bool existingCase = await _caseRepository.ExistingOpenAttendanceCaseForStudent(student.Id, cancellationToken);

            response.Add(new(
                studentEntries.Key,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolCode,
                enrolment.SchoolName,
                period,
                existingCase,
                studentEntries.FirstOrDefault()?.PerMinuteWeekPercentage ?? 100,
                studentEntries.Skip(1).FirstOrDefault()?.PerMinuteWeekPercentage ?? 100,
                studentEntries.Skip(2).FirstOrDefault()?.PerMinuteWeekPercentage ?? 100,
                studentEntries.Skip(3).FirstOrDefault()?.PerMinuteWeekPercentage ?? 100,
                studentEntries.LastOrDefault()?.PerMinuteWeekPercentage ?? 100,
                AttendanceSeverity.FromAttendanceValue(studentEntries.FirstOrDefault()?.PerMinuteWeekPercentage ?? 100)));
        }

        return response;
    }
}
