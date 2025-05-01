namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateCaseStatus;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateCaseStatusCommandHandler
: ICommandHandler<UpdateCaseStatusCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateCaseStatusCommandHandler(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCaseStatusCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateCaseStatusCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to update Case Status");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        CaseStatus currentStatus = item.Status;

        Result update = item.UpdateStatus(request.Status, _currentUserService.UserName);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateCaseStatusCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Case Status");

            return Result.Failure(update.Error);
        }

        Staff currentUser = await _staffRepository.GetCurrentByEmailAddress(_currentUserService.EmailAddress, cancellationToken);

        if (currentUser is null)
        {
            _logger
                .ForContext(nameof(UpdateCaseStatusCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress), true)
                .Warning("Failed to update Case Status");

            return Result.Failure(DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress));
        }

        Result<CaseDetailUpdateAction> updateAction = CaseDetailUpdateAction.Create(
            null,
            item.Id,
            currentUser,
            $"Case status changed from {currentStatus} to {request.Status} by {_currentUserService.UserName} at {_dateTime.Now}");

        if (updateAction.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateCaseStatusCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), updateAction.Error, true)
                .Warning("Failed to update Case Status");

            return Result.Failure(updateAction.Error);
        }

        item.AddAction(updateAction.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
