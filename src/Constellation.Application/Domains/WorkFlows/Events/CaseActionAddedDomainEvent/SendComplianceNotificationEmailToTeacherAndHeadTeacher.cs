namespace Constellation.Application.Domains.WorkFlows.Events.CaseActionAddedDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Extensions;
using Interfaces.Configuration;
using Interfaces.Gateways;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

internal sealed class SendComplianceNotificationEmailToTeacherAndHeadTeacher
: IDomainEventHandler<CaseActionAddedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IEmailService _emailService;
    private readonly ISentralGateway _sentralGateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly AppConfiguration _configuration;
    private readonly SentralGatewayConfiguration _sentralConfiguration;
    private readonly ILogger _logger;

    public SendComplianceNotificationEmailToTeacherAndHeadTeacher(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IEmailService emailService,
        ISentralGateway sentralGateway,
        IDateTimeProvider dateTime,
        IOptions<SentralGatewayConfiguration> sentralConfiguration,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _emailService = emailService;
        _sentralGateway = sentralGateway;
        _dateTime = dateTime;
        _configuration = configuration.Value;
        _sentralConfiguration = sentralConfiguration.Value;
        _logger = logger.ForContext<CaseActionAddedDomainEvent>();
    }

    public async Task Handle(CaseActionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        if (!item.Type!.Equals(CaseType.Compliance))
            return;

        Action action = item.Actions.FirstOrDefault(entry => entry.Id == notification.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to Assignee for new Action");

            return;
        }

        if (action is not CaseDetailUpdateAction)
            return;

        ComplianceCaseDetail detail = item.Detail as ComplianceCaseDetail;
        
        StaffMember assignee = await _staffRepository.GetById(detail!.CreatedById, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(detail.CreatedById), true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.Name, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(StaffMember), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        recipients.Add(teacher.Value);

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(detail.CreatedById, cancellationToken);

        foreach (Faculty faculty in faculties)
        {
            if (faculty.Name is "Administration" or "Executive" or "Support")
                continue;

            List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

            foreach (StaffMember headTeacher in headTeachers)
            {
                if (recipients.Any(entry => entry.Email == headTeacher.EmailAddress.Email))
                    continue;

                Result<EmailRecipient> headTeacherEmail = EmailRecipient.Create(headTeacher.Name, headTeacher.EmailAddress);
                if (headTeacherEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(StaffMember), headTeacher, true)
                        .ForContext(nameof(Error), headTeacherEmail.Error, true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                recipients.Add(headTeacherEmail.Value);
            }
        }

        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(_dateTime.CurrentYearAsString);
        List<DateOnly> datesBetween = detail.CreatedDate.Range(_dateTime.Today);
        datesBetween = datesBetween
            .Where(entry => !excludedDates.Contains(entry))
            .Where(entry =>
                entry.DayOfWeek != DayOfWeek.Saturday &&
                entry.DayOfWeek != DayOfWeek.Sunday)
            .ToList();

        int age = datesBetween.Count - 1;

        if (age < 12 || (age - 12) % 5 != 0)
            return;

        if (age >= 17)
        {
            // Add DP to recipients
            List<EmployeeId> deputies = _configuration.Contacts.DeputyPrincipalIds[detail.Grade];

            foreach (EmployeeId deputyId in deputies)
            {
                StaffMember deputy = await _staffRepository.GetByEmployeeId(deputyId, cancellationToken);

                if (deputy is null)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(deputyId), true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                if (recipients.Any(entry => entry.Email == deputy.EmailAddress.Email))
                    continue;

                Result<EmailRecipient> deputyEmail = EmailRecipient.Create(assignee.Name, assignee.EmailAddress);
                if (deputyEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(StaffMember), deputy, true)
                        .ForContext(nameof(Error), deputyEmail.Error, true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                recipients.Add(deputyEmail.Value);
            }
        }

        if (age >= 22)
        {
            // Add P to recipients
            EmployeeId principalId = _configuration.Contacts.PrincipalId;
            
            StaffMember principal = await _staffRepository.GetByEmployeeId(principalId, cancellationToken);

            if (principal is null)
            {
                _logger
                    .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                    .ForContext(nameof(Error), StaffMemberErrors.NotFound(detail.CreatedById), true)
                    .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                return;
            }

            if (recipients.All(entry => entry.Email != principal.EmailAddress.Email))
            {
                Result<EmailRecipient> principalEmail = EmailRecipient.Create(principal.Name, principal.EmailAddress);
                if (principalEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(StaffMember), principal, true)
                        .ForContext(nameof(Error), principalEmail.Error, true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                recipients.Add(principalEmail.Value);
            }
        }

        string incidentLink = $"{_sentralConfiguration.ServerUrl}/wellbeing/incidents/view?id={detail.IncidentId}";

        await _emailService.SendComplianceWorkFlowNotificationEmail(recipients, item.Id, assignee.Name, detail, age, incidentLink, cancellationToken);
    }
}
