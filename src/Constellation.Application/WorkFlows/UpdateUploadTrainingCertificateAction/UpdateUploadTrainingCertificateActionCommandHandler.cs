namespace Constellation.Application.WorkFlows.UpdateUploadTrainingCertificateAction;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateUploadTrainingCertificateActionCommandHandler
: ICommandHandler<UpdateUploadTrainingCertificateActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpdateUploadTrainingCertificateActionCommandHandler(
        ICaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger.ForContext<UpdateUploadTrainingCertificateActionCommand>();
    }

    public async Task<Result> Handle(UpdateUploadTrainingCertificateActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateUploadTrainingCertificateActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to update Action");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(UpdateUploadTrainingCertificateActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), ActionErrors.NotFound(request.ActionId), true)
                .Warning("Failed to update Action");

            return Result.Failure(ActionErrors.NotFound(request.ActionId));
        }

        if (action is not UploadTrainingCertificateAction uploadAction)
        {
            _logger
                .ForContext(nameof(UploadTrainingCertificateAction), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), ActionErrors.UpdateTypeMismatch(nameof(UploadTrainingCertificateAction), action.GetType().ToString()), true)
                .Warning("Failed to update Action");

            return Result.Failure(ActionErrors.UpdateTypeMismatch(nameof(UploadTrainingCertificateAction), action.GetType().ToString()));
        }

        Result updateAction = uploadAction.Update(_currentUserService.UserName);

        if (updateAction.IsFailure)
        {
            _logger
            .ForContext(nameof(UploadTrainingCertificateAction), request, true)
            .ForContext(nameof(Case), item, true)
            .ForContext(nameof(Error), updateAction.Error, true)
            .Warning("Failed to update Action");

            return Result.Failure(updateAction.Error);
        }

        Result statusUpdate = item.UpdateActionStatus(action.Id, ActionStatus.Completed, _currentUserService.UserName);

        if (statusUpdate.IsFailure)
        {
            _logger
                .ForContext(nameof(UploadTrainingCertificateAction), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), statusUpdate.Error, true)
                .Warning("Failed to update Action");

            return Result.Failure(statusUpdate.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
