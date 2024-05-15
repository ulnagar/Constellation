namespace Constellation.Application.WorkFlows.CancelAction;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CancelActionCommandHandler
: ICommandHandler<CancelActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CancelActionCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CancelActionCommand>();
    }

    public async Task<Result> Handle(CancelActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CancelActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.Case.NotFound(request.CaseId), true)
                .Warning("Failed to mark Action as cancelled");

            return Result.Failure(CaseErrors.Case.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CancelActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), CaseErrors.Action.NotFound(request.ActionId), true)
                .Warning("Failed to mark Action as cancelled");

            return Result.Failure(CaseErrors.Action.NotFound(request.ActionId));
        }

        Result update = action.AssignedTo == _currentUserService.UserName
            ? item.UpdateActionStatus(action.Id, ActionStatus.Cancelled, _currentUserService.UserName)
            : item.UpdateActionStatus(action.Id, ActionStatus.Cancelled, _currentUserService.UserName, false);


        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(CancelActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to mark Action as cancelled");

            return Result.Failure(update.Error);
        }

        List<Action> subActions = item.Actions
            .Where(entry => 
                entry.ParentActionId == action.Id &&
                entry.Status.Equals(ActionStatus.Open))
            .ToList();

        foreach (Action entry in subActions)
        {
            Result subActionUpdate = entry.AssignedTo == _currentUserService.UserName
                ? item.UpdateActionStatus(entry.Id, ActionStatus.Cancelled, _currentUserService.UserName)
                : item.UpdateActionStatus(entry.Id, ActionStatus.Cancelled, _currentUserService.UserName, false);
            
            if (subActionUpdate.IsFailure)
            {
                _logger
                    .ForContext(nameof(CancelActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), subActionUpdate.Error, true)
                    .Warning("Failed to mark Action as cancelled");

                return Result.Failure(subActionUpdate.Error);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
