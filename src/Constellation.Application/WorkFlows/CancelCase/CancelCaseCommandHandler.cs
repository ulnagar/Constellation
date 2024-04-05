namespace Constellation.Application.WorkFlows.CancelCase;

using Abstractions.Messaging;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CancelCaseCommandHandler
: ICommandHandler<CancelCaseCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CancelCaseCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelCaseCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CancelCaseCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.Case.NotFound(request.CaseId), true)
                .Warning("Failed to mark Case as cancelled");

            return Result.Failure(CaseErrors.Case.NotFound(request.CaseId));
        }

        Result update = item.UpdateStatus(CaseStatus.Cancelled, _currentUserService.UserName);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(CancelCaseCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to mark Case as cancelled");

            return Result.Failure(update.Error);
        }

        Staff currentUser = await _staffRepository.GetCurrentByEmailAddress(_currentUserService.EmailAddress, cancellationToken);

        if (currentUser is null)
        {
            _logger
                .ForContext(nameof(CancelCaseCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress), true)
                .Warning("Failed to mark Case as cancelled");

            return Result.Failure(DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress));
        }

        Result<CaseDetailUpdateAction> updateAction = CaseDetailUpdateAction.Create(
            null,
            item.Id,
            currentUser,
            $"Case cancelled by {_currentUserService.UserName} at {_dateTime.Now}");

        if (updateAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CancelCaseCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), updateAction.Error, true)
                .Warning("Failed to mark Case as cancelled");

            return Result.Failure(updateAction.Error);
        }

        item.AddAction(updateAction.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
