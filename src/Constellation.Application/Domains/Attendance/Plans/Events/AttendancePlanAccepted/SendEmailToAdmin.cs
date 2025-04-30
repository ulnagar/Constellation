namespace Constellation.Application.Domains.Attendance.Plans.Events.AttendancePlanAccepted;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Events;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendEmailToAdmin
: IDomainEventHandler<AttendancePlanAcceptedDomainEvent>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendEmailToAdmin(
        IAttendancePlanRepository planRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _planRepository = planRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<AttendancePlanAcceptedDomainEvent>();
    }

    public async Task Handle(AttendancePlanAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(notification.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(notification.PlanId), true)
                .Warning("Failed to send attendance plan email to school admin");

            return;
        }

        Result<EmailRecipient> recipient = EmailRecipient.Create("Aurora College", "auroracoll-h.school@det.nsw.edu.au");

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send attendance plan email to school admin");

            return;
        }

        await _emailService.SendAttendancePlanToAdmin([recipient.Value], plan, cancellationToken);
    }
}
