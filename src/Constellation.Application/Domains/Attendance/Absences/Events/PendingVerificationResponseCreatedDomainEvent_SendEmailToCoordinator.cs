namespace Constellation.Application.Domains.Attendance.Absences.Events;

using Commands.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students.Errors;
using MediatR;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator
    : INotificationHandler<PendingVerificationResponseCreatedDomainEvent>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<PendingVerificationResponseCreatedDomainEvent>();
    }

    public async Task Handle(PendingVerificationResponseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Send email to ACC to let them know there is a response that requires verification.
        Absence absence = await _absenceRepository.GetById(notification.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not locate absence with Id {id}", notification.AbsenceId);
            return;
        }

        Response response = absence.Responses.FirstOrDefault(response => response.Id == notification.ResponseId);

        if (response is null)
        {
            _logger.Warning("Could not locate absence response with Id {id}", notification.ResponseId);
            return;
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not locate student {studentId} related to absence with Id {id}", absence.StudentId, absence.Id);
            return;
        }

        SchoolEnrolment enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(PendingVerificationResponseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Error("Could not retrieve current school details for student {studentId}", absence.StudentId);
            
            return;
        }

        List<SchoolContact> coordinators = await _contactRepository.GetBySchoolAndRole(enrolment.SchoolCode, Position.Coordinator, cancellationToken);

        List<EmailRecipient> recipients = new();

        foreach (SchoolContact coordinator in coordinators)
        {
            Result<Name> nameResult = Name.Create(coordinator.FirstName, string.Empty, coordinator.LastName);
            if (nameResult.IsFailure)
                continue;

            Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, coordinator.EmailAddress);

            if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                recipients.Add(result.Value);
        }

        School school = await _schoolRepository.GetById(enrolment.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger.Warning("Could not find School with Id {id}", enrolment.SchoolCode);

            return;
        }

        if (recipients.Count == 0)
        {
            Result<EmailRecipient> result = EmailRecipient.Create(school.Name, school.EmailAddress);

            if (result.IsSuccess)
                recipients.Add(result.Value);
        }

        if (recipients.Count == 0)
        {
            _logger.Warning("No recipients could be found or created: {school}", school);

            return;
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not locate course offering with Id {id}", absence.OfferingId);

            return;
        }

        AbsenceExplanation explanation = new(
            absence.Date, 
            absence.PeriodName, 
            absence.PeriodTimeframe, 
            offering.Name, 
            absence.AbsenceTimeframe, 
            response.Explanation);

        EmailDtos.SentEmail message = await _emailService.SendCoordinatorPartialAbsenceVerificationRequest(
            new List<AbsenceExplanation> { explanation },
            student,
            recipients,
            cancellationToken);

        if (message == null)
            return;

        string emails = string.Join(", ", recipients.Select(entry => entry.Email));

        absence.AddNotification(
            NotificationType.Email,
            message.message,
            emails,
            message.id,
            _dateTime.Now);

        foreach (EmailRecipient recipient in recipients)
            _logger.Information("Partial Absence Verification Request send to {recipient} for absence by {student} on {date}", recipient.Name, student.Name.DisplayName, absence.Date);
    }
}
