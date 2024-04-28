namespace Constellation.Application.Attendance.GetAttendanceTrendValues;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceTrendValuesQueryHandler
: IQueryHandler<GetAttendanceTrendValuesQuery, List<AttendanceTrend>>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public GetAttendanceTrendValuesQueryHandler(
        ICaseRepository caseRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IAttendanceRepository attendanceRepository)
    {
        _caseRepository = caseRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<Result<List<AttendanceTrend>>> Handle(GetAttendanceTrendValuesQuery request, CancellationToken cancellationToken)
    {
        List<AttendanceTrend> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<AttendanceValue> values = await _attendanceRepository.GetAllRecent(cancellationToken);

        IEnumerable<IGrouping<string, AttendanceValue>> groupedValues = values.OrderByDescending(value => value.EndDate).GroupBy(value => value.StudentId);

        foreach (IGrouping<string, AttendanceValue> studentEntries in groupedValues)
        {
            Student student = students.FirstOrDefault(entry => entry.StudentId == studentEntries.Key);

            if (student is null)
                continue;

            School school = schools.FirstOrDefault(entry => entry.Code == student.SchoolCode);

            if (school is null) 
                continue;

            string period = studentEntries.First().PeriodLabel;

            bool existingCase = await _caseRepository.ExistingOpenAttendanceCaseForStudent(student.StudentId, cancellationToken);

            response.Add(new(
                studentEntries.Key,
                student.GetName(),
                student.CurrentGrade,
                student.SchoolCode,
                school.Name,
                period,
                existingCase,
                studentEntries.First().PerMinuteWeekPercentage,
                studentEntries.Skip(1).First().PerMinuteWeekPercentage,
                studentEntries.Skip(2).First().PerMinuteWeekPercentage,
                studentEntries.Skip(3).First().PerMinuteWeekPercentage,
                studentEntries.Last().PerMinuteWeekPercentage,
                AttendanceSeverity.FromAttendanceValue(studentEntries.First().PerMinuteWeekPercentage)));
        }

        return response;
    }
}
