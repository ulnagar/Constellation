namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Configuration;
using Interfaces.Repositories;
using Microsoft.Extensions.Options;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddPhoneParentActionForBandThreeAttendanceCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public AddPhoneParentActionForBandThreeAttendanceCase(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository,
        IOptions<AppConfiguration> configuration,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
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
                .Warning("Could not create default Action for new Case");

            return;
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return;

        AttendanceCaseDetail caseDetail = item.Detail as AttendanceCaseDetail;

        if (!caseDetail!.Severity.Equals(AttendanceSeverity.BandThree))
            return;

        StaffMember reviewer = await _staffRepository.GetByEmployeeId(_configuration.WorkFlow.AttendanceReviewer, cancellationToken);

        if (reviewer is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(_configuration.WorkFlow.AttendanceReviewer), true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        Result<PhoneParentAction> phoneAction = PhoneParentAction.Create(item.Id, reviewer, _currentUserService.UserName);

        if (phoneAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), phoneAction.Error, true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        item.AddAction(phoneAction.Value);
        
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
