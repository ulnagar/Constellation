namespace Constellation.Application.WorkFlows.Events.CaseActionAddedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculties;
using Constellation.Core.Models.Faculties.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Events;
using Constellation.Core.Models.WorkFlow.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        
        Staff assignee = await _staffRepository.GetById(detail!.CreatedById, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(detail.CreatedById), true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

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
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        recipients.Add(teacher.Value);

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(detail.CreatedById, cancellationToken);

        foreach (Faculty faculty in faculties)
        {
            if (faculty.Name is "Administration" or "Executive" or "Support")
                continue;

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
            List<string> deputies = _configuration.Contacts.DeputyPrincipalIds[detail.Grade];

            foreach (string deputyId in deputies)
            {
                Staff deputy = await _staffRepository.GetById(deputyId, cancellationToken);

                if (deputy is null)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(deputyId), true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                if (recipients.Any(entry => entry.Email == deputy.EmailAddress))
                    continue;

                Result<EmailRecipient> deputyEmail = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
                if (deputyEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(Staff), deputy, true)
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
            string principalId = _configuration.Contacts.PrincipalId;
            
            Staff principal = await _staffRepository.GetById(principalId, cancellationToken);

            if (principal is null)
            {
                _logger
                    .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(detail.CreatedById), true)
                    .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                return;
            }

            if (recipients.All(entry => entry.Email != principal.EmailAddress))
            {
                Result<EmailRecipient> principalEmail = EmailRecipient.Create(principal.DisplayName, principal.EmailAddress);
                if (principalEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseActionAddedDomainEvent), notification, true)
                        .ForContext(nameof(Staff), principal, true)
                        .ForContext(nameof(Error), principalEmail.Error, true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                recipients.Add(principalEmail.Value);
            }
        }

        string incidentLink = $"{_sentralConfiguration.ServerUrl}/wellbeing/incidents/view?id={detail.IncidentId}";

        await _emailService.SendComplianceWorkFlowNotificationEmail(recipients, item.Id, detail, age, incidentLink, cancellationToken);
    }
}
