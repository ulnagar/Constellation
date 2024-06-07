﻿namespace Constellation.Application.Assets.CreateAsset;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assets;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAssetCommandHandler
: ICommandHandler<CreateAssetCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateAssetCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<CreateAssetCommand>();
    }

    public async Task<Result> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        Result<Asset> asset = await Asset.Create(
            request.AssetNumber,
            request.SerialNumber,
            request.Manufacturer,
            request.Description,
            request.Category,
            _dateTime,
            _assetRepository);

        if (asset.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateAssetCommand), request, true)
                .ForContext(nameof(Error), asset.Error, true)
                .Warning("Failed to create new Asset");

            return Result.Failure(asset.Error);
        }

        _assetRepository.Insert(asset.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
