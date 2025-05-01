namespace Constellation.Application.Domains.WorkFlows.Events.CaseActionCompletedDomainEvent;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationToCaseOwnerWhenAllActionsCompleted
    : IDomainEventHandler<CaseActionCompletedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly AppConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationToCaseOwnerWhenAllActionsCompleted(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IOptions<AppConfiguration> configuration,
        IEmailService emailService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _configuration = configuration.Value;
        _emailService = emailService;
        _logger = logger.ForContext<SendNotificationToCaseOwnerWhenAllActionsCompleted>();
    }

    public async Task Handle(CaseActionCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        if (item.Actions.Any(action => action.Status.Equals(ActionStatus.Open)))
            return;

        string assigneeId = item.Type switch
        {
            not null when item.Type.Equals(CaseType.Attendance) => _configuration.WorkFlow.AttendanceReviewer,
            not null when item.Type.Equals(CaseType.Compliance) => _configuration.WorkFlow.ComplianceReviewer,
            not null when item.Type.Equals(CaseType.Training) => _configuration.WorkFlow.TrainingReviewer,
            _ => null
        };

        Staff assignee = assigneeId switch
        {
            not null => await _staffRepository.GetById(assigneeId, cancellationToken),
            _ => null
        };

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(assigneeId), true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Staff), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        recipients.Add(teacher.Value);

        await _emailService.SendAllActionsCompletedEmail(recipients, item, cancellationToken);
    }
}