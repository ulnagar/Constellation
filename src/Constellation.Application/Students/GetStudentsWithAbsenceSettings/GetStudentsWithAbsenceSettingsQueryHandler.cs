namespace Constellation.Application.Students.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsWithAbsenceSettingsQueryHandler
    : IQueryHandler<GetStudentsWithAbsenceSettingsQuery, List<StudentAbsenceSettingsResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsWithAbsenceSettingsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentAbsenceSettingsResponse>>> Handle(GetStudentsWithAbsenceSettingsQuery request, CancellationToken cancellationToken)
    {
        List<StudentAbsenceSettingsResponse> returnData = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        if (students.Count == 0)
            return returnData;

        foreach(var student in students)
        {
            List<StudentAbsenceSettingsResponse.AbsenceConfigurationResponse> absenceConfigurationResponses = new();

            foreach (AbsenceConfiguration configuration in student.AbsenceConfigurations)
            {
                if (configuration.IsDeleted || configuration.CalendarYear != DateTime.Today.Year)
                    continue;

                absenceConfigurationResponses.Add(new(
                    configuration.AbsenceType,
                    configuration.ScanStartDate,
                    configuration.ScanEndDate));
            }

            StudentAbsenceSettingsResponse entry = new(
                student.StudentId,
                student.DisplayName,
                student.Gender,
                student.CurrentGrade,
                student.School.Name,
                absenceConfigurationResponses);

            returnData.Add(entry);
        }

        return returnData;
    }
}
