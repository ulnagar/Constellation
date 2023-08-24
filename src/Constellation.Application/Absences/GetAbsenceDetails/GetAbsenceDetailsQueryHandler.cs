namespace Constellation.Application.Absences.GetAbsenceDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsQueryHandler
    : IQueryHandler<GetAbsenceDetailsQuery, AbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetAbsenceDetailsQuery>();
    }

    public async Task<Result<AbsenceDetailsResponse>> Handle(
        GetAbsenceDetailsQuery request,
        CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not locate absence with Id {id}", request.AbsenceId);

            return Result.Failure<AbsenceDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not locate student with Id {id}", absence.StudentId);

            return Result.Failure<AbsenceDetailsResponse>(DomainErrors.Partners.Student.NotFound(absence.StudentId));        
        }

        Result<Name> studentNameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

        if (studentNameRequest.IsFailure)
        {
            _logger.Warning("Could not create Name object from student with Id {id}", absence.StudentId);

            return Result.Failure<AbsenceDetailsResponse>(studentNameRequest.Error);
        }

        School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger.Warning("Could not locate school with Id {id}", student.SchoolCode);

            return Result.Failure<AbsenceDetailsResponse>(DomainErrors.Partners.School.NotFound(student.SchoolCode));
        }

        List<AbsenceDetailsResponse.AbsenceResponseDetails> convertedResponses = new();

        foreach (Response response in absence.Responses)
        {
            AbsenceDetailsResponse.AbsenceResponseDetails entry = new(
                response.Id,
                response.Type,
                response.Explanation,
                response.VerificationStatus,
                response.ReceivedAt,
                response.Verifier,
                response.VerificationComment,
                response.VerifiedAt);

            convertedResponses.Add(entry);
        }

        List<AbsenceDetailsResponse.AbsenceNotificationDetails> convertedNotifications = new();

        foreach (Notification notification in absence.Notifications)
        {
            AbsenceDetailsResponse.AbsenceNotificationDetails entry = new(
                notification.Id,
                notification.Type,
                notification.Recipients,
                notification.Message,
                notification.SentAt);

            convertedNotifications.Add(entry);
        }

        AbsenceDetailsResponse result = new(
            absence.Id,
            student.StudentId,
            studentNameRequest.Value,
            student.CurrentGrade,
            school.Name,
            absence.Type,
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceLength,
            absence.AbsenceTimeframe,
            convertedNotifications,
            convertedResponses);

        return result;
    }
}
