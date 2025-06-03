namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.TransferAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class TransferAssetToCoordinatingOfficeCommandHandler
: ICommandHandler<TransferAssetToCoordinatingOfficeCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public TransferAssetToCoordinatingOfficeCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<TransferAssetToCoordinatingOfficeCommand>();
    }

    public async Task<Result> Handle(TransferAssetToCoordinatingOfficeCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(TransferAssetToCoordinatingOfficeCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to transfer asset to Coordinating Office");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result<Location> location = Location.CreateOfficeLocationRecord(
            asset.Id, 
            request.Room, 
            request.CurrentLocation, 
            request.ArrivalDate);

        if (location.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferAssetToCoordinatingOfficeCommand), request, true)
                .ForContext(nameof(Error), location.Error, true)
                .Warning("Failed to transfer asset to Coordinating Office");

            return Result.Failure(location.Error);
        }

        Result result = asset.AddLocation(location.Value);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to transfer asset to Coordinating Office");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
