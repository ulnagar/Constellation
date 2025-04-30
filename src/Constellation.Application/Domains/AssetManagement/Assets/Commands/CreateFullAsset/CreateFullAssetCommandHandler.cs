namespace Constellation.Application.Assets.CreateFullAsset;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assets;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateFullAssetCommandHandler
: ICommandHandler<CreateFullAssetCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateFullAssetCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<CreateFullAssetCommand>();
    }

    public async Task<Result> Handle(CreateFullAssetCommand request, CancellationToken cancellationToken)
    {
        Result<Asset> asset = await Asset.Create(
            request.AssetNumber,
            request.SerialNumber,
            request.Manufacturer,
            request.ModelDescription,
            request.Category,
            _dateTime,
            _assetRepository);

        if (asset.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateFullAssetCommand), request, true)
                .ForContext(nameof(Error), asset.Error, true)
                .Warning("Failed to create new full Asset");

            return Result.Failure(asset.Error);
        }

        Result result = asset.Value.Update(
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
                .ForContext(nameof(CreateFullAssetCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create new full Asset");

            return Result.Failure(result.Error);
        }

        _assetRepository.Insert(asset.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
