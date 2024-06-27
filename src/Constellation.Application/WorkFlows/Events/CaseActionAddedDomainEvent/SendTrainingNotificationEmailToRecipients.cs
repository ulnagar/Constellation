namespace Constellation.Application.WorkFlows.Events.CaseActionAddedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculties;
using Constellation.Core.Models.Faculties.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Events;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendTrainingNotificationEmailToRecipients
    : IDomainEventHandler<CaseActionAddedDomainEvent>
{
    private readonly AppConfiguration _configuration;
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendTrainingNotificationEmailToRecipients(
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
        _logger = logger.ForContext<SendTrainingNotificationEmailToRecipients>();
    }

    public async Task Handle(CaseActionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        if (!item.Type!.Equals(CaseType.Training))
            return;

        Action action = item.Actions.FirstOrDefault(entry => entry.Id == notification.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        if (action is not CaseDetailUpdateAction)
            return;

        TrainingCaseDetail detail = item.Detail as TrainingCaseDetail;

        Staff assignee = await _staffRepository.GetById(detail!.StaffId, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(detail.StaffId), true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Staff), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        recipients.Add(teacher.Value);

        string reviewerId = _configuration.WorkFlow.TrainingReviewer;

        Staff reviewer = await _staffRepository.GetById(reviewerId, cancellationToken);

        if (reviewer is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(reviewerId), true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        Result<EmailRecipient> reviewerEmail = EmailRecipient.Create(reviewer.DisplayName, reviewer.EmailAddress);
        if (reviewerEmail.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Staff), reviewer, true)
                .ForContext(nameof(Error), reviewerEmail.Error, true)
                .Warning("Could not send notification to recipients for Training Case update");

            return;
        }

        recipients.Add(reviewerEmail.Value);

        if (detail.DaysUntilDue <= 14)
        {
            List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(detail.StaffId, cancellationToken);

            foreach (Faculty faculty in faculties)
            {
                List<Staff> headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

                foreach (Staff headTeacher in headTeachers)
                {
                    if (recipients.Any(entry => entry.Email == headTeacher.EmailAddress))
                        continue;

                    Result<EmailRecipient> headTeacherEmail = EmailRecipient.Create(headTeacher.DisplayName, headTeacher.EmailAddress);
                    if (headTeacherEmail.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                            .ForContext(nameof(Staff), headTeacher, true)
                            .ForContext(nameof(Error), headTeacherEmail.Error, true)
                            .Warning("Could not send notification to recipients for Training Case update");

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
                List<SchoolContact> sharedSchoolPrincipals = await _contactRepository.GetPrincipalsForSchool(assignee.SchoolCode, cancellationToken);

                foreach (SchoolContact sharedPrincipal in sharedSchoolPrincipals)
                {
                    if (recipients.Any(entry => entry.Email == sharedPrincipal.EmailAddress))
                        continue;

                    Result<EmailRecipient> sharedPrincipalEmail = EmailRecipient.Create(sharedPrincipal.DisplayName, sharedPrincipal.EmailAddress);
                    if (sharedPrincipalEmail.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                            .ForContext(nameof(Staff), sharedPrincipal, true)
                            .ForContext(nameof(Error), sharedPrincipalEmail.Error, true)
                            .Warning("Could not send notification to recipients for Training Case update");

                        return;
                    }

                    recipients.Add(sharedPrincipalEmail.Value);
                }
            }

            string principalId = _configuration.Contacts.PrincipalId;

            Staff principal = await _staffRepository.GetById(principalId, cancellationToken);

            if (principal is null)
            {
                _logger
                    .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(principalId), true)
                    .Warning("Could not send notification to recipients for Training Case update");

                return;
            }

            if (recipients.All(entry => entry.Email != principal.EmailAddress))
            {
                Result<EmailRecipient> principalEmail =
                    EmailRecipient.Create(principal.DisplayName, principal.EmailAddress);
                if (principalEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(Staff), principal, true)
                        .ForContext(nameof(Error), principalEmail.Error, true)
                        .Warning("Could not send notification to recipients for Training Case update");

                    return;
                }

                recipients.Add(principalEmail.Value);
            }
        }
        
        await _emailService.SendTrainingWorkFlowNotificationEmail(recipients, detail, reviewer.DisplayName, cancellationToken);
    }
}