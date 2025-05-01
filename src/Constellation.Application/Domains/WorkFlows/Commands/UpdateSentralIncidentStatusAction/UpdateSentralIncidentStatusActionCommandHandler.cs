namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateSentralIncidentStatusAction;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateSentralIncidentStatusActionCommandHandler
: ICommandHandler<UpdateSentralIncidentStatusActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpdateSentralIncidentStatusActionCommandHandler(
        ICaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger.ForContext<UpdateSentralIncidentStatusActionCommand>();
    }

    public async Task<Result> Handle(UpdateSentralIncidentStatusActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to update Action");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), request, true)
            .ForContext(nameof(Case), item, true)
            .ForContext(nameof(Error), ActionErrors.NotFound(request.ActionId), true)
            .Warning("Failed to update Action");

            return Result.Failure(ActionErrors.NotFound(request.ActionId));
        }

        if (action is SentralIncidentStatusAction incidentAction)
        {
            Result updateAction = request switch
            {
                _ when request.MarkResolved => incidentAction.Update(_currentUserService.UserName),
                _ when request.MarkNotCompleted => incidentAction.Update(request.IncidentNumber, _currentUserService.UserName),
                _ => Result.Failure(ActionErrors.InvalidOption(nameof(SentralIncidentStatusAction)))
            };
            
            if (updateAction.IsFailure)
            {
                _logger
                .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), updateAction.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(updateAction.Error);
            }

            Result statusUpdate = item.UpdateActionStatus(action.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (statusUpdate.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), statusUpdate.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(statusUpdate.Error);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        _logger
            .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), request, true)
            .ForContext(nameof(Case), item, true)
            .ForContext(nameof(Error), ActionErrors.UpdateTypeMismatch(nameof(ParentInterviewAction), action.GetType().ToString()), true)
            .Warning("Failed to update Action");

        return Result.Failure(ActionErrors.UpdateTypeMismatch(nameof(ParentInterviewAction), action.GetType().ToString()));
    }
}
