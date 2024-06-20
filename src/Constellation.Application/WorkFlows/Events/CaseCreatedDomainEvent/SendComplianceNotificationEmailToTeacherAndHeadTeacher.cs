namespace Constellation.Application.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models.Faculties.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.WorkFlow.Events;
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

internal sealed class SendComplianceNotificationEmailToTeacherAndHeadTeacher
: IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IEmailService _emailService;
    private readonly ISentralGateway _sentralGateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly SentralGatewayConfiguration _configuration;
    private readonly ILogger _logger;

    public SendComplianceNotificationEmailToTeacherAndHeadTeacher(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IEmailService emailService,
        ISentralGateway sentralGateway,
        IDateTimeProvider dateTime,
        IOptions<SentralGatewayConfiguration> configuration,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _emailService = emailService;
        _sentralGateway = sentralGateway;
        _dateTime = dateTime;
        _configuration = configuration.Value;
        _logger = logger.ForContext<CaseCreatedDomainEvent>();
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        if (!item.Type!.Equals(CaseType.Compliance))
            return;

        ComplianceCaseDetail detail = item.Detail as ComplianceCaseDetail;
        
        Staff assignee = await _staffRepository.GetById(detail!.CreatedById, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(detail.CreatedById), true)
                .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
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
                        .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                        .ForContext(nameof(Staff), headTeacher, true)
                        .ForContext(nameof(Error), headTeacherEmail.Error, true)
                        .Warning("Could not send notification to teacher and head teacher for new Compliance Action");

                    return;
                }

                recipients.Add(headTeacherEmail.Value);
            }
        }

        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(_dateTime.CurrentYear.ToString());
        List<DateOnly> datesBetween = detail.CreatedDate.Range(_dateTime.Today);
        datesBetween = datesBetween
            .Where(entry => !excludedDates.Contains(entry))
            .Where(entry =>
                entry.DayOfWeek != DayOfWeek.Saturday &&
                entry.DayOfWeek != DayOfWeek.Sunday)
            .ToList();

        int age = datesBetween.Count - 1;

        // TODO: R1.15: Add extra recipients as required
        if (age >= 17)
        {
            // Add DP to recipients
        }

        if (age >= 22)
        {
            // Add P to recipients
        }

        string incidentLink = $"{_configuration.ServerUrl}/wellbeing/incidents/view?id={detail.IncidentId}";

        await _emailService.SendComplianceWorkFlowNotificationEmail(recipients, item.Id, detail, age, incidentLink, cancellationToken);
    }
}
