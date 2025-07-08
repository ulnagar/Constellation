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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSendEmailActionForAttendanceCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public AddSendEmailActionForAttendanceCase(
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

        List<AttendanceSeverity> severityList = new()
        {
            AttendanceSeverity.BandZero,
            AttendanceSeverity.BandTwo,
            AttendanceSeverity.BandThree,
            AttendanceSeverity.BandFour
        };

        if (!severityList.Contains(caseDetail!.Severity))
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

        Result<SendEmailAction> emailAction = SendEmailAction.Create(item.Id, reviewer, _currentUserService.UserName);

        if (emailAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), emailAction.Error, true)
                .Warning("Could not create default Action for new Case");

            return;
        }
        
        item.AddAction(emailAction.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
