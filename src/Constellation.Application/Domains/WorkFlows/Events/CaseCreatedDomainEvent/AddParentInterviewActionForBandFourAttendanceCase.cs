namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using AppSettings.Models;
using Core.Abstractions.Services;
using Core.Models.AppSettings.Enums;
using Core.Models.StaffMembers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddParentInterviewActionForBandFourAttendanceCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppSettingsService _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddParentInterviewActionForBandFourAttendanceCase(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IAppSettingsService appSettings,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CaseCreatedDomainEvent>();
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case? item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

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

        AttendanceCaseDetail? caseDetail = item.Detail as AttendanceCaseDetail;

        if (!caseDetail!.Severity.Equals(AttendanceSeverity.BandFour))
            return;
        
        WorkflowConfiguration? reviewers = await _appSettings.Workflow(WorkflowArea.Attendance, cancellationToken);

        if (reviewers is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        StaffMember? staffMember = null;

        foreach (var reviewer in reviewers.Contacts)
        {
            if (!reviewer.Value.Contains(caseDetail.Grade))
                continue;

            staffMember = reviewer.Key;
        }

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .Warning("Could not create default Action for new Case");

            return;
        }
        
        Result<ParentInterviewAction> interviewAction = ParentInterviewAction.Create(item.Id, staffMember, _currentUserService.UserName);

        if (interviewAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), interviewAction.Error, true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        item.AddAction(interviewAction.Value);
        
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
