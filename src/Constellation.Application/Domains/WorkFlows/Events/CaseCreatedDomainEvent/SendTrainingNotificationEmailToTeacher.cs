namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendTrainingNotificationEmailToTeacher
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly AppConfiguration _configuration;
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendTrainingNotificationEmailToTeacher(
        IOptions<AppConfiguration> configuration,
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _configuration = configuration.Value;
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SendTrainingNotificationEmailToTeacher>();
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

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

        TrainingCaseDetail detail = item.Detail as TrainingCaseDetail;

        StaffMember assignee = await _staffRepository.GetById(detail!.StaffId, cancellationToken);

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

        EmployeeId reviewerId = _configuration.WorkFlow.TrainingReviewer;

        StaffMember reviewer = await _staffRepository.GetByEmployeeId(reviewerId, cancellationToken);

        if (reviewer is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(reviewerId), true)
                .Warning("Could not send notification to teacher for new Training Action");

            return;
        }

        Result<EmailRecipient> reviewerEmail = EmailRecipient.Create(reviewer.Name, reviewer.EmailAddress);
        if (reviewerEmail.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(StaffMember), reviewer, true)
                .ForContext(nameof(Error), reviewerEmail.Error, true)
                .Warning("Could not send notification to teacher for new Training Action");

            return;
        }

        recipients.Add(reviewerEmail.Value);

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
                string schoolCode = assignee.CurrentAssignment?.SchoolCode ?? null;

                if (schoolCode is not null)
                {
                    List<SchoolContact> sharedSchoolPrincipals = await _contactRepository.GetPrincipalsForSchool(schoolCode, cancellationToken);

                    foreach (SchoolContact sharedPrincipal in sharedSchoolPrincipals)
                    {
                        if (recipients.Any(entry => entry.Email == sharedPrincipal.EmailAddress))
                            continue;

                        Result<EmailRecipient> sharedPrincipalEmail = EmailRecipient.Create(sharedPrincipal.DisplayName, sharedPrincipal.EmailAddress);
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

            EmployeeId principalId = _configuration.Contacts.PrincipalId;

            StaffMember principal = await _staffRepository.GetByEmployeeId(principalId, cancellationToken);

            if (principal is null)
            {
                _logger
                    .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                    .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(principalId), true)
                    .Warning("Could not send notification to teacher for new Training Action");

                return;
            }

            if (recipients.All(entry => entry.Email != principal.EmailAddress.Email))
            {
                Result<EmailRecipient> principalEmail = EmailRecipient.Create(principal.Name, principal.EmailAddress);
                if (principalEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                        .ForContext(nameof(StaffMember), principal, true)
                        .ForContext(nameof(Error), principalEmail.Error, true)
                        .Warning("Could not send notification to teacher for new Training Action");

                    return;
                }

                recipients.Add(principalEmail.Value);
            }
        }
        
        await _emailService.SendTrainingWorkFlowNotificationEmail(recipients, detail, reviewer.Name, cancellationToken);
    }
}