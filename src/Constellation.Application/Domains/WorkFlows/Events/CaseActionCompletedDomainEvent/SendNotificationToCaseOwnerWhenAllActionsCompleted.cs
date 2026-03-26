namespace Constellation.Application.Domains.WorkFlows.Events.CaseActionCompletedDomainEvent;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Core.Models.Tutorials.Events;
using Core.Models.AppSettings.Enums;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Extensions;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationToCaseOwnerWhenAllActionsCompleted
    : IDomainEventHandler<CaseActionCompletedDomainEvent>
{
    private readonly IAppSettingsService _appSettings;
    private readonly ICaseRepository _caseRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationToCaseOwnerWhenAllActionsCompleted(
        IAppSettingsService appSettings,
        ICaseRepository caseRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _appSettings = appSettings;
        _caseRepository = caseRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SendNotificationToCaseOwnerWhenAllActionsCompleted>();
    }

    public async Task Handle(CaseActionCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case? item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

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
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .Warning("Could not send notification to Case Assignee for new Status");

            return;
        }

        foreach (var contact in configuration.Contacts)
        {
            Result<EmailRecipient> contactEmail = contact.Key.GetEmailRecipient;

            if (contactEmail.IsFailure)
            {
                _logger
                    .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                    .ForContext(nameof(Error), contactEmail.Error, true)
                    .Warning("Failed to send notification email for Tutorial Request");

                return;
            }

            recipients.Add(contactEmail.Value);
        }
        
        if (recipients.Count > 0)
            await _emailService.SendAllActionsCompletedEmail(recipients, item, cancellationToken);
    }
}