namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;

public class AbsenceMonitorJob : IAbsenceMonitorJob, IHangfireJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbsenceProcessingJob _absenceProcessor;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public AbsenceMonitorJob(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository schoolContactRepository,
        ICourseOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        IAbsenceProcessingJob absenceProcessor,
        IEmailService emailService,
        ISMSService smsService,
        IMediator mediator,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _schoolContactRepository = schoolContactRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _absenceProcessor = absenceProcessor;
        _emailService = emailService;
        _smsService = smsService;
        _mediator = mediator;
        _logger = logger.ForContext<IAbsenceMonitorJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Information("{id}: Starting Absence Monitor Scan.", jobId);

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Getting students from {grade}", jobId, grade);

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            students = students
                .OrderBy(student => student.LastName)
                .ThenBy(student => student.FirstName)
                .ToList();

            _logger.Information("{id}: Found {students} students to scan.", jobId, students.Count);

            foreach (Student student in students)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                List<Absence> absences = await _absenceProcessor.StartJob(jobId, student, cancellationToken);

                if (absences.Any())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    foreach (Absence absence in absences)
                        _absenceRepository.Insert(absence);

                    if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendStudentNotifications(
                        jobId, 
                        student, 
                        absences.Where(absence => absence.Type == AbsenceType.Partial).ToList(), 
                        cancellationToken);

                    await _mediator.Send(new SendAbsenceNotificationToParentCommand(
                        jobId,
                        student.StudentId,
                        absences.Where(absence => absence.Type == AbsenceType.Whole).Select(absence => absence.Id).ToList()),
                        cancellationToken);
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendParentDigests(jobId, student, cancellationToken);
                    await SendCoordinatorDigests(jobId, student, cancellationToken);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
                absences = null;
            }
        }
    }

    private async Task SendCoordinatorDigests(
        Guid jobId, 
        Student student,
        CancellationToken cancellationToken)
    {
        List<Absence> digestAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 2, cancellationToken);

        if (digestAbsences.Any())
        {
            List<SchoolContact> coordinators = await _schoolContactRepository.GetBySchoolAndRole(student.SchoolCode, SchoolContactRole.Coordinator, cancellationToken);

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

            List<AbsenceEntry> absenceEntries = new();

            foreach (Absence absence in digestAbsences)
            {
                CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                if (offering is null)
                    continue;

                absenceEntries.Add(new(
                    absence.Id,
                    absence.Date,
                    absence.PeriodName,
                    absence.PeriodTimeframe,
                    offering.Name,
                    absence.AbsenceTimeframe));
            }

            EmailDtos.SentEmail message = await _emailService.SendCoordinatorWholeAbsenceDigest(absenceEntries, student, recipients, cancellationToken);

            if (message == null)
                return;

            string emails = string.Join(", ", recipients.Select(entry => entry.Email));

            foreach (var absence in digestAbsences)
            {
                absence.AddNotification(
                    NotificationType.Email,
                    message.message,
                    emails);

                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: School digest sent to {recipient} for {student}", jobId, recipient.Name, student.DisplayName);
            }

            absenceEntries = null;
            coordinators = null;
            recipients = null;
            digestAbsences = null;
        }
    }

    private async Task SendParentDigests(
        Guid jobId, 
        Student student, 
        CancellationToken cancellationToken = default)
    {
        List<Absence> digestAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);

        if (digestAbsences.Any())
        {
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
            List<Parent> parents = families.SelectMany(family => family.Parents).ToList();
            List<EmailRecipient> recipients = new();

            foreach (var family in families)
            {
                Result<EmailRecipient> result = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (result.IsSuccess)
                    recipients.Add(result.Value);
            }

            foreach (var parent in parents)
            {
                Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

                if (nameResult.IsFailure)
                    continue;

                Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            if (recipients.Any())
            {
                List<AbsenceEntry> absenceEntries = new();

                foreach (Absence absence in digestAbsences)
                {
                    CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                    if (offering is null)
                        continue;

                    absenceEntries.Add(new(
                        absence.Id,
                        absence.Date,
                        absence.PeriodName,
                        absence.PeriodTimeframe,
                        offering.Name,
                        absence.AbsenceTimeframe));
                }

                EmailDtos.SentEmail sentmessage = await _emailService.SendParentWholeAbsenceDigest(absenceEntries, student, recipients, cancellationToken);

                if (sentmessage == null)
                    return;

                string emails = string.Join(", ", recipients.Select(entry => entry.Email));

                foreach (Absence absence in digestAbsences)
                {
                    absence.AddNotification(
                        NotificationType.Email,
                        sentmessage.message,
                        emails);

                    foreach (var recipient in recipients)
                        _logger.Information("{id}: Parent digest sent to {address} for {student}", jobId, recipient.Email, student.DisplayName);
                }

                absenceEntries = null;
            }
            else
            {
                await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
            }

            families = null;
            parents = null;
            recipients = null;
            digestAbsences = null;
        }
    }

    private async Task SendStudentNotifications(
        Guid jobId, 
        Student student, 
        List<Absence> partialAbsences, 
        CancellationToken cancellationToken = default)
    {
        // Send emails to students
        if (partialAbsences.Any())
        {
            List<EmailRecipient> recipients = new();

            Result<EmailRecipient> result = EmailRecipient.Create(student.DisplayName, student.EmailAddress);

            if (result.IsSuccess)
            {
                recipients.Add(result.Value);
            }

            List<AbsenceEntry> absenceEntries = new();

            foreach (Absence absence in partialAbsences)
            {
                CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                if (offering is null)
                    continue;

                absenceEntries.Add(new(
                    absence.Id,
                    absence.Date,
                    absence.PeriodName,
                    absence.PeriodTimeframe,
                    offering.Name,
                    absence.AbsenceTimeframe));
            }

            EmailDtos.SentEmail sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(absenceEntries, student, recipients, cancellationToken);

            foreach (Absence absence in partialAbsences)
            {
                absence.AddNotification(NotificationType.Email, sentEmail.message, student.EmailAddress);

                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: Message sent via Email to {student} ({email}) for Partial Absence on {Date}", jobId, recipient.Name, recipient.Email, absence.Date.ToShortDateString());
            }

            recipients = null;
            partialAbsences = null;
            absenceEntries = null;
        }
    }
}
