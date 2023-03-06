namespace Constellation.Application.Students.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
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
        var returnData = new List<StudentAbsenceSettingsResponse>();

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        if (students is null)
        {
            return Result.Failure<List<StudentAbsenceSettingsResponse>>(DomainErrors.Partners.Student.NotFound("00000000"));
        }

        foreach(var student in students)
        {
            var entry = new StudentAbsenceSettingsResponse(
                student.StudentId,
                student.DisplayName,
                student.Gender,
                student.CurrentGrade,
                student.School.Name,
                student.IncludeInAbsenceNotifications,
                (student.IncludeInAbsenceNotifications) ? DateOnly.FromDateTime(student.AbsenceNotificationStartDate.Value) : null);

            returnData.Add(entry);
        }

        return returnData;
    }
}
