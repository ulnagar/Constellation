namespace Constellation.Application.Domains.Compliance.Assessments.Commands.SendNotificationsForAssessmentProvisions;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Enums;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using DTOs.EmailRequests;
using Interfaces;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationsForAssessmentProvisionsCommandHandler
: ICommandHandler<SendNotificationsForAssessmentProvisionsCommand>
{
    private readonly IAssessmentProvisionsCacheService _cacheService;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SendNotificationsForAssessmentProvisionsCommandHandler(
        IAssessmentProvisionsCacheService cacheService,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _cacheService = cacheService;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<SendNotificationsForAssessmentProvisionsCommand>();
    }

    public async Task<Result> Handle(SendNotificationsForAssessmentProvisionsCommand request, CancellationToken cancellationToken)
    {
        List<StudentProvisions> studentProvisions = _cacheService.GetRecords().Where(record =>
            record.StudentEmail == StudentProvisions.EmailStatus.Pending ||
            record.SchoolEmail == StudentProvisions.EmailStatus.Pending).ToList();

        foreach (StudentProvisions studentProvision in studentProvisions)
        {
            if (studentProvision.StudentEmail != StudentProvisions.EmailStatus.Pending)
                continue;

            List<EmailRecipient> recipients = [];
            List<EmailRecipient> ccRecipients = [];

            foreach (var offering in studentProvision.OfferingProvisions)
            {
                List<StaffMember> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offering.OfferingId, cancellationToken);

                foreach (var teacher in teachers)
                {
                    Result<EmailRecipient> recipient = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

                    if (recipient.IsSuccess)
                        ccRecipients.Add(recipient.Value);
                }

                List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(offering.OfferingId, cancellationToken);

                foreach (var teacher in headTeachers)
                {
                    Result<EmailRecipient> recipient = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

                    if (recipient.IsSuccess)
                        ccRecipients.Add(recipient.Value);
                }

                Student student = await _studentRepository.GetById(studentProvision.StudentId, cancellationToken);

                if (student is null)
                {
                    return Result.Failure(StudentErrors.NotFound(studentProvision.StudentId));
                }

                Result<EmailRecipient> studentEmail = EmailRecipient.Create(student.Name, student.EmailAddress);

                if (studentEmail.IsSuccess)
                    recipients.Add(studentEmail.Value);

                List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

                families = families.Where(family => 
                    family.Students
                        .Any(membership =>
                                membership.StudentId == student.Id && 
                                membership.IsResidentialFamily))
                    .ToList();

                foreach (Family family in families)
                {
                    Result<EmailRecipient> familyRecipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                    if (familyRecipient.IsSuccess)
                        recipients.Add(familyRecipient.Value);

                    foreach (Parent parent in family.Parents)
                    {
                        Result<EmailRecipient> parentRecipient = EmailRecipient.Create($"{parent.FirstName} {parent.LastName}", parent.EmailAddress);

                        if (parentRecipient.IsSuccess)
                            recipients.Add(parentRecipient.Value);
                    }
                }
            }

            recipients = recipients.Distinct().ToList();
            ccRecipients = ccRecipients.Distinct().ToList();
            
            Result studentEmailResult = await _emailService.SendAssessmentProvisionEmailToFamilies(recipients, ccRecipients, studentProvision, cancellationToken);

            studentProvision.StudentEmail = 
                studentEmailResult.IsFailure 
                    ? StudentProvisions.EmailStatus.Error 
                    : StudentProvisions.EmailStatus.Sent;
        }

        List<StudentId> studentIds = studentProvisions
            .Where(record => record.SchoolEmail == StudentProvisions.EmailStatus.Pending)
            .Select(student => student.StudentId)
            .ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        IEnumerable<IGrouping<string, Student>> studentsBySchool = students.GroupBy(student => student.CurrentEnrolment?.SchoolCode);

        foreach (IGrouping<string, Student> schoolGroup in studentsBySchool)
        {
            List<StudentId> schoolStudentIds = schoolGroup.Select(student => student.Id).ToList();

            List<StudentProvisions> schoolAdjustmentsList = studentProvisions
                .Where(adjustment => schoolStudentIds.Contains(adjustment.StudentId))
                .ToList();

            List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(schoolGroup.Key, Position.Coordinator, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
                Result<Name> contactName = contact.GetName();

                Result<EmailRecipient> recipient = contact.GetEmailRecipient();

                if (recipient.IsFailure || contactName.IsFailure)
                {
                    foreach (var provision in schoolAdjustmentsList)
                    {
                        provision.SchoolEmail = StudentProvisions.EmailStatus.Error;
                    }

                    continue;
                }

                Result schoolEmailResult = await _emailService.SendAssessmentProvisionEmailToSchools([recipient.Value], [], contactName.Value, schoolAdjustmentsList, cancellationToken);

                foreach (var provision in schoolAdjustmentsList)
                {
                    provision.SchoolEmail = schoolEmailResult.IsFailure
                        ? StudentProvisions.EmailStatus.Error
                        : StudentProvisions.EmailStatus.Sent;
                }
            }
        }

        List<StudentProvisions> errors = studentProvisions
            .Where(record =>
                record.StudentEmail == StudentProvisions.EmailStatus.Error ||
                record.SchoolEmail == StudentProvisions.EmailStatus.Error)
            .ToList();

        if (errors.Count > 0)
            return await SendServiceLogEmail(errors);

        return Result.Success();
    }

    private async Task<Result> SendServiceLogEmail(List<StudentProvisions> errors)
    {
        List<string> messages =
        [
            "The following Assessment Provision emails failed to send:",
            string.Empty
        ];

        foreach (var error in errors)
        {
            if (error.StudentEmail == StudentProvisions.EmailStatus.Error)
                messages.Add($"{error.Student} - Email to Student and Family");

            if (error.SchoolEmail == StudentProvisions.EmailStatus.Error)
                messages.Add($"{error.Student} - Email to Partner School");
        }

        Result<EmailRecipient> emailRecipient = EmailRecipient.Create(_currentUserService.UserName, _currentUserService.EmailAddress);

        if (emailRecipient.IsFailure)
        {
            _logger
                .Error("Failed to send error list to user");

            return Result.Failure(ApplicationErrors.UnknownError);
        }

        await _emailService.SendServiceLogEmail(new ServiceLogEmail()
            {
                Log = messages,
                Recipients = [emailRecipient.Value],
                Source = nameof(SendNotificationsForAssessmentProvisionsCommand)
            });

        return Result.Success();
    }
}
