namespace Constellation.Application.Assets.UpdateAsset;

using Abstractions.Messaging;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAssetCommandHandler
: ICommandHandler<UpdateAssetCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAssetCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateAssetCommand>();
    }

    public async Task<Result> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(UpdateAssetCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to update Asset");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result result = asset.Update(
            request.Manufacturer,
            request.ModelNumber,
            request.ModelDescription,
            request.SapEquipmentNumber,
            request.PurchaseDate,
            request.PurchaseDocument,
            request.PurchaseCost,
            request.WarrantyEndDate);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateAssetCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Asset");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
