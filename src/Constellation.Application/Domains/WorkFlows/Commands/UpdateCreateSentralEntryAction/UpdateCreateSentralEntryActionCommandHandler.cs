namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateCreateSentralEntryAction;

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

internal sealed class UpdateCreateSentralEntryActionCommandHandler
: ICommandHandler<UpdateCreateSentralEntryActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateCreateSentralEntryActionCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateCreateSentralEntryActionCommand>();
    }

    public async Task<Result> Handle(UpdateCreateSentralEntryActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateCreateSentralEntryActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to update Action");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(UpdateCreateSentralEntryActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), ActionErrors.NotFound(request.ActionId), true)
                .Warning("Failed to update Action");

            return Result.Failure(ActionErrors.NotFound(request.ActionId));
        }

        if (action is CreateSentralEntryAction sentralAction)
        {
            Result updateAction = request.NotRequired switch
            {
                true => sentralAction.Update(request.NotRequired, _currentUserService.UserName),
                false => sentralAction.Update(request.IncidentNumber, _currentUserService.UserName)
            };

            if (updateAction.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateCreateSentralEntryActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), updateAction.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(updateAction.Error);
            }

            Result statusUpdate = item.UpdateActionStatus(action.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (statusUpdate.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateCreateSentralEntryActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), statusUpdate.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(statusUpdate.Error);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        _logger
            .ForContext(nameof(UpdateCreateSentralEntryActionCommand), request, true)
            .ForContext(nameof(Case), item, true)
            .ForContext(nameof(Error), ActionErrors.UpdateTypeMismatch(nameof(CreateSentralEntryAction), action.GetType().ToString()), true)
            .Warning("Failed to update Action");

        return Result.Failure(ActionErrors.UpdateTypeMismatch(nameof(CreateSentralEntryAction), action.GetType().ToString()));
    }
}
