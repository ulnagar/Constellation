namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Application.Extensions;
using Constellation.Core.Models.AppSettings.Enums;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendTrainingNotificationEmailToTeacher
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly IAppSettingsService _appSettings;
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendTrainingNotificationEmailToTeacher(
        IAppSettingsService appSettings,
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _appSettings = appSettings;
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SendTrainingNotificationEmailToTeacher>();
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case? item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to teacher for new Training Action");

            return;
        }

        if (!item.Type!.Equals(CaseType.Training))
            return;

        TrainingCaseDetail? detail = item.Detail as TrainingCaseDetail;

        StaffMember? assignee = await _staffRepository.GetById(detail!.StaffId, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(detail.StaffId), true)
                .Warning("Could not send notification to teacher for new Training Action");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.Name, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(StaffMember), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send notification to teacher for new Training Action");

            return;
        }

        recipients.Add(teacher.Value);

        WorkflowConfiguration? reviewers = await _appSettings.Workflow(WorkflowArea.Training, cancellationToken);

        if (reviewers is not null)
        {
            foreach (var reviewer in reviewers.Contacts)
            {
                Result<EmailRecipient> reviewerEmail = reviewer.Key.GetEmailRecipient();

                if (reviewerEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                        .ForContext(nameof(StaffMember), reviewer, true)
                        .ForContext(nameof(Error), reviewerEmail.Error, true)
                        .Warning("Could not send notification to teacher for new Training Action");

                    continue;
                }

                recipients.Add(reviewerEmail.Value);
            }
        }
        
        if (detail.DaysUntilDue <= 14)
        {
            List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(detail.StaffId, cancellationToken);

            foreach (Faculty faculty in faculties)
            {
                List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

                foreach (StaffMember headTeacher in headTeachers)
                {
                    if (recipients.Any(entry => entry.Email == headTeacher.EmailAddress.Email))
                        continue;

                    Result<EmailRecipient> headTeacherEmail = EmailRecipient.Create(headTeacher.Name, headTeacher.EmailAddress);
                    if (headTeacherEmail.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                            .ForContext(nameof(StaffMember), headTeacher, true)
                            .ForContext(nameof(Error), headTeacherEmail.Error, true)
                            .Warning("Could not send notification to teacher for new Training Action");

                        return;
                    }

                    recipients.Add(headTeacherEmail.Value);
                }
            }
        }

        if (detail.DaysUntilDue <= 0)
        {
            // Add Principal

            if (assignee.IsShared)
            {
                string? schoolCode = assignee.CurrentAssignment?.SchoolCode ?? null;

                if (schoolCode is not null)
                {
                    List<SchoolContact> sharedSchoolPrincipals = await _contactRepository.GetPrincipalsForSchool(schoolCode, cancellationToken);

                    foreach (SchoolContact sharedPrincipal in sharedSchoolPrincipals)
                    {
                        if (recipients.Any(entry => entry.Email == sharedPrincipal.EmailAddress.Email))
                            continue;

                        Result<EmailRecipient> sharedPrincipalEmail = EmailRecipient.Create(sharedPrincipal.Name, sharedPrincipal.EmailAddress);
                        if (sharedPrincipalEmail.IsFailure)
                        {
                            _logger
                                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                                .ForContext(nameof(StaffMember), sharedPrincipal, true)
                                .ForContext(nameof(Error), sharedPrincipalEmail.Error, true)
                                .Warning("Could not send notification to teacher for new Training Action");

                            return;
                        }

                        recipients.Add(sharedPrincipalEmail.Value);
                    }
                }
            }

            ContactsConfiguration? principals = await _appSettings.Contacts(ContactPosition.Principal, cancellationToken);

            if (principals is not null)
            {
                foreach (var principal in principals.Contacts)
                {
                    Result<EmailRecipient> principalEmail = principal.Key.GetEmailRecipient();

                    if (principalEmail.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                            .ForContext(nameof(StaffMember), principal.Key, true)
                            .ForContext(nameof(Error), principalEmail.Error, true)
                            .Warning("Could not send notification to recipients for Training Case update");

                        return;
                    }

                    if (recipients.Any(entry => entry.Email == principalEmail.Value.Email))
                        continue;

                    recipients.Add(principalEmail.Value);
                }
            }
        }
        
        await _emailService.SendTrainingWorkFlowNotificationEmail(recipients, detail, cancellationToken);
    }
}