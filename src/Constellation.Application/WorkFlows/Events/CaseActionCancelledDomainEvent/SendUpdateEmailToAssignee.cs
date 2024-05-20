namespace Constellation.Application.WorkFlows.Events.CaseActionCancelledDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Events;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendUpdateEmailToAssignee
: IDomainEventHandler<CaseActionCancelledDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendUpdateEmailToAssignee(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _logger = logger.ForContext<CaseActionCancelledDomainEvent>();
    }

    public async Task Handle(CaseActionCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send cancellation notification to Assignee for Action");

            return;
        }

        Action action = item.Actions.FirstOrDefault(entry => entry.Id == notification.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not send cancellation notification to Assignee for Action");

            return;
        }

        Staff assignee = await _staffRepository.GetById(action.AssignedToId, cancellationToken);

        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .ForContext(nameof(Action), action, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(action.AssignedToId), true)
                .Warning("Could not send cancellation notification to Assignee for Action");

            return;
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> teacher = EmailRecipient.Create(assignee.DisplayName, assignee.EmailAddress);
        if (teacher.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionCancelledDomainEvent), notification, true)
                .ForContext(nameof(Staff), assignee, true)
                .ForContext(nameof(Error), teacher.Error, true)
                .Warning("Could not send cancellation notification to Assignee for Action");

            return;
        }

        recipients.Add(teacher.Value);

        await _emailService.SendActionCancelledEmail(recipients, item, action, assignee, cancellationToken);
    }
}
