namespace Constellation.Application.Domains.Students.Queries.GetStudentsWithAbsenceSettings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Absences.Enums;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsWithAbsenceSettingsQueryHandler
    : IQueryHandler<GetStudentsWithAbsenceSettingsQuery, List<StudentAbsenceSettingsResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetStudentsWithAbsenceSettingsQueryHandler(
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime)
    {
        _studentRepository = studentRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<List<StudentAbsenceSettingsResponse>>> Handle(GetStudentsWithAbsenceSettingsQuery request, CancellationToken cancellationToken)
    {
        List<StudentAbsenceSettingsResponse> returnData = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students.Count == 0)
            return returnData;

        foreach(var student in students)
        {
            bool activeWhole = false;
            bool activePartial = false;

            List<StudentAbsenceSettingsResponse.AbsenceConfigurationResponse> absenceConfigurationResponses = new();

            foreach (AbsenceConfiguration configuration in student.AbsenceConfigurations)
            {
                if (configuration.IsDeleted || configuration.CalendarYear != DateTime.Today.Year)
                    continue;

                if (_dateTime.Today >= configuration.ScanStartDate && _dateTime.Today <= configuration.ScanEndDate)
                {
                    if (configuration.AbsenceType == AbsenceType.Whole)
                        activeWhole = true;

                    if (configuration.AbsenceType == AbsenceType.Partial)
                        activePartial = true;
                }

                absenceConfigurationResponses.Add(new(
                    configuration.AbsenceType,
                    configuration.ScanStartDate,
                    configuration.ScanEndDate));
            }

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            StudentAbsenceSettingsResponse entry = new(
                student.Id,
                student.StudentReferenceNumber,
                student.Name.DisplayName,
                student.PreferredGender.Value,
                enrolment.Grade,
                enrolment.SchoolName,
                absenceConfigurationResponses,
                activeWhole,
                activePartial);

            returnData.Add(entry);
        }

        return returnData;
    }
}
