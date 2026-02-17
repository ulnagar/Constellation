namespace Constellation.Application.Domains.WorkFlows.Events.CaseActionCancelledDomainEvent;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Application.Extensions;
using Constellation.Core.Models.AppSettings.Enums;
using Core.Models.StaffMembers;
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

internal sealed class SendNotificationToCaseOwnerWhenAllActionsCancelled
    : IDomainEventHandler<CaseActionCancelledDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationToCaseOwnerWhenAllActionsCancelled(
        ICaseRepository caseRepository,
        IAppSettingsService appSettings,
        IEmailService emailService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _appSettings = appSettings;
        _emailService = emailService;
        _logger = logger.ForContext<SendNotificationToCaseOwnerWhenAllActionsCancelled>();
    }

    public async Task Handle(CaseActionCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        Case? item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        if (item.Actions.Any(action => action.Status.Equals(ActionStatus.Open)))
            return;

        WorkflowConfiguration? configuration = item.Type switch
        {
            not null when item.Type.Equals(CaseType.Attendance) => await _appSettings.Workflow(WorkflowArea.Attendance, cancellationToken),
            not null when item.Type.Equals(CaseType.Compliance) => await _appSettings.Workflow(WorkflowArea.Compliance, cancellationToken),
            not null when item.Type.Equals(CaseType.Training) => await _appSettings.Workflow(WorkflowArea.Training, cancellationToken),
            _ => null
        };

        List<EmailRecipient> recipients = [];

        if (configuration is null)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        foreach (var contact in configuration.Contacts)
        {
            Result<EmailRecipient> contactEmail = contact.Key.GetEmailRecipient();

            if (contactEmail.IsFailure)
            {
                _logger
                    .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                    .ForContext(nameof(StaffMember), contact.Key, true)
                    .ForContext(nameof(Error), contactEmail.Error, true)
                    .Warning("Could not send notification to Case Assignee for new Status");

                continue;
            }

            recipients.Add(contactEmail.Value);
        }

        if (recipients.Count > 0)
            await _emailService.SendAllActionsCompletedEmail(recipients, item, cancellationToken);
    }
}