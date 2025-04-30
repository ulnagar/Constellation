namespace Constellation.Application.Domains.Attendance.Plans.Events.AttendancePlanRejected;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Errors;
using Constellation.Core.Models.Attendance.Repositories;
using Core.Models.Attendance.Events;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


internal sealed class SendEmailNotificationToSchool
: IDomainEventHandler<AttendancePlanRejectedDomainEvent>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendEmailNotificationToSchool(
        IAttendancePlanRepository planRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _planRepository = planRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(AttendancePlanRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        // If there was no notification for the school selected, skip this action.
        if (!notification.NotifySchool)
            return;

        AttendancePlan plan = await _planRepository.GetById(notification.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanRejectedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(notification.PlanId), true)
                .Warning("Failed to send Attendance Plan Rejected notification");

            return;
        }

        List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(plan.SchoolCode, Position.Coordinator, cancellationToken);

        List<EmailRecipient> recipients = contacts
            .Select(contact => contact.GetEmailRecipient())
            .Where(entry => entry.IsSuccess)
            .Select(entry => entry.Value)
            .ToList();

        if (recipients.Count == 0)
        {
            _logger
                .ForContext(nameof(AttendancePlanRejectedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFoundForSchoolAndRole(plan.School, Position.Coordinator), true)
                .Warning("Failed to send Attendance Plan Rejected notification");

            return;
        }

        await _emailService.SendAttendancePlanRejectedNotificationToSchool(recipients, plan, notification.Comment, cancellationToken);
    }
}