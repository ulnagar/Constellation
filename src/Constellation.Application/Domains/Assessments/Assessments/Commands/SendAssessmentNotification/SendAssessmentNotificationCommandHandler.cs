namespace Constellation.Application.Domains.Assessments.Assessments.Commands.SendAssessmentNotification;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings.Enums;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;

internal sealed class SendAssessmentNotificationCommandHandler
: ICommandHandler<SendAssessmentNotificationCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendAssessmentNotificationCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        IAssessmentRepository assessmentRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _assessmentRepository = assessmentRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<SendAssessmentNotificationCommand>();
    }

    public async Task<Result> Handle(SendAssessmentNotificationCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(SendAssessmentNotificationCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to send Assessment Notification");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<EmailRecipient> recipients = [];

        List<StudentId> studentIds = request.IncludeStudents || request.IncludeParents
            ? assessment.Students.Select(entry => entry.StudentId).Distinct().ToList()
            : [];

        List<SchoolCode> schoolCodes = request.IncludeSchoolContacts
            ? assessment.Students.Select(entry => entry.SchoolCode).Distinct().ToList()
            : [];

        List<Offering> classes = request.IncludeClassroomTeachers
            ? await _offeringRepository.GetActiveByCourseId(assessment.CourseId, cancellationToken)
            : [];

        if (request.IncludeParents)
        {
            foreach (StudentId studentId in studentIds)
            {
                List<Family> families = await _familyRepository.GetFamiliesByStudentId(studentId, cancellationToken);

                List<Parent> parents = families.SelectMany(entry => entry.Parents).ToList();

                foreach (Parent parent in parents)
                {
                    if (parent.EmailAddress == EmailAddress.None)
                        continue;

                    if (recipients.Any(entry => entry.Email == parent.EmailAddress))
                        continue;

                    Result<EmailRecipient> recipient = EmailRecipient.Create(parent.Name, parent.EmailAddress);

                    if (recipient.IsFailure)
                        continue;

                    recipients.Add(recipient.Value);
                }
            }
        }

        if (request.IncludeStudents)
        {
            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            foreach (Student student in students)
            {
                if (student.EmailAddress == EmailAddress.None)
                    continue;

                if (recipients.Any(entry => entry.Email == student.EmailAddress))
                    continue;

                Result<EmailRecipient> recipient = EmailRecipient.Create(student.Name, student.EmailAddress);

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }

        if (request.IncludeSchoolContacts)
        {
            foreach (SchoolCode schoolCode in schoolCodes)
            {
                List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(schoolCode, Position.Coordinator, cancellationToken);

                foreach (SchoolContact contact in contacts)
                {
                    if (contact.EmailAddress == EmailAddress.None)
                        continue;

                    if (recipients.Any(entry => entry.Email == contact.EmailAddress))
                        continue;

                    Result<EmailRecipient> recipient = EmailRecipient.Create(contact.Name, contact.EmailAddress);

                    if (recipient.IsFailure)
                        continue;

                    recipients.Add(recipient.Value);
                }
            }
        }

        if (request.IncludeClassroomTeachers)
        {
            List<StaffId> staffIds = classes
                .SelectMany(entry => entry.Teachers)
                .Where(entry => entry.Type == AssignmentType.ClassroomTeacher)
                .Select(entry => entry.StaffId)
                .ToList();

            List<StaffMember> teachers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

            foreach (StaffMember teacher in teachers)
            {
                if (teacher.EmailAddress == EmailAddress.None)
                    continue;

                if (recipients.Any(entry => entry.Email == teacher.EmailAddress))
                    continue;

                Result<EmailRecipient> recipient = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }
        
        Dictionary<Result, List<EmailRecipient>> result = await _emailService.SendAssessmentNotification(assessment, recipients, cancellationToken);

        List<KeyValuePair<Result, List<EmailRecipient>>> failedEmails = result
            .Where(entry => entry.Key.IsFailure)
            .ToList();

        if (failedEmails.Count > 0)
        {
            foreach (var group in failedEmails)
            {
                _logger
                    .ForContext(nameof(SendAssessmentNotificationCommand), request, true)
                    .ForContext(nameof(EmailRecipient), group.Value, true)
                    .ForContext(nameof(Error), group.Key.Error, true)
                    .Error("Failed to send Assessment Notification");
            }

            return Result.Failure(ApplicationErrors.UnknownError);
        }

        return Result.Success();
    }
}
