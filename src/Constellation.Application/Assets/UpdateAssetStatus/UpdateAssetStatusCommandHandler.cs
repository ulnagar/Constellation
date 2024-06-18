﻿namespace Constellation.Application.Assets.UpdateAssetStatus;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAssetStatusCommandHandler
: ICommandHandler<UpdateAssetStatusCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAssetStatusCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateAssetStatusCommand>();
    }

    public async Task<Result> Handle(UpdateAssetStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.AssetStatus is null)
        {
            _logger
                .ForContext(nameof(UpdateAssetStatusCommand), request, true)
                .ForContext(nameof(Error), ApplicationErrors.ArgumentNull(nameof(AssetStatus)), true)
                .Warning("Failed to update Asset Status");

            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(AssetStatus)));
        }

        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(UpdateAssetStatusCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to update Asset Status");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result result = asset.UpdateStatus(request.AssetStatus);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateAssetStatusCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Asset Status");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
