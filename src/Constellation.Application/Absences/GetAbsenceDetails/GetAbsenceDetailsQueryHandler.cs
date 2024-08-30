namespace Constellation.Application.Absences.GetAbsenceDetails;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsQueryHandler
    : IQueryHandler<GetAbsenceDetailsQuery, AbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsQueryHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
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

            return Result.Failure<AbsenceDetailsResponse>(StudentErrors.NotFound(absence.StudentId));        
        }

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger.Warning("Could not retrieve current School Enrolment for student with id {Id}", absence.StudentId);

            return Result.Failure<AbsenceDetailsResponse>(SchoolEnrolmentErrors.NotFound);
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
            student.Id,
            student.Name,
            enrolment.Grade,
            enrolment.SchoolName,
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
