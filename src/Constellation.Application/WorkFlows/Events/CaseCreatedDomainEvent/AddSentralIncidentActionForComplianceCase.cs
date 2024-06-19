namespace Constellation.Application.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSentralIncidentActionForComplianceCase
: IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddSentralIncidentActionForComplianceCase(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
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
                .Warning("Could not create default Action for new Case");

            return;
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return;

        ComplianceCaseDetail caseDetail = item.Detail as ComplianceCaseDetail;

        Staff teacher = await _staffRepository.GetById(caseDetail.CreatedById, cancellationToken);
        if (teacher is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(caseDetail.CreatedById), true)
                .Warning("Could not create default Action for new Case");
            return;
        }
        
        Result<SentralIncidentStatusAction> action = SentralIncidentStatusAction.Create(item.Id, teacher, _currentUserService.UserName);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        item.AddAction(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
