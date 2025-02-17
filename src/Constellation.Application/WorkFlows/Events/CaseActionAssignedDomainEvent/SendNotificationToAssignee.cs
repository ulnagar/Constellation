namespace Constellation.Application.WorkFlows.Events.CaseActionAssignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Events;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Errors;
using Core.Models.WorkFlow.Enums;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationToAssignee
    : IDomainEventHandler<CaseActionAssignedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationToAssignee(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _logger = logger.ForContext<CaseActionAssignedDomainEvent>();
    }

    public async Task Handle(CaseActionAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionAssignedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to Assignee for new Action");

            return;
        }

        Action action = item.Actions.FirstOrDefault(entry => entry.Id == notification.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CaseActionAssignedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send notification to Assignee for new Action");

            return;
        }

        // Do not send an email if the action was auto-closed
        if (action.Status != ActionStatus.Open)
            return;

        Staff assignee = await _staffRepository.GetById(action.AssignedToId, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionAssignedDomainEvent), notification, true)
                .ForContext(nameof(Action), action, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(action.AssignedToId), true)
                .Warning("Could not send notification to Assignee for new Action");

            return;
        }
        
        List<EmailRecipient> recipients = new();
        
        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionAssignedDomainEvent), notification, true)
                .ForContext(nameof(Staff), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send notification to Assignee for new Action");

            return;
        }

        recipients.Add(teacher.Value);

        await _emailService.SendActionAssignedEmail(recipients, item, action, assignee, cancellationToken);
    }
}
