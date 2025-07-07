namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingWithAssetRecordUpdates;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Stocktake.Enums;
using Constellation.Core.Models.Stocktake.Errors;
using Constellation.Core.Models.Stocktake.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterSightingWithAssetRecordUpdatesCommandHandler
: ICommandHandler<RegisterSightingWithAssetRecordUpdatesCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RegisterSightingWithAssetRecordUpdatesCommandHandler(
        IAssetRepository assetRepository,
        IStocktakeRepository stocktakeRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _stocktakeRepository = stocktakeRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RegisterSightingWithAssetRecordUpdatesCommand>();
    }

    public async Task<Result> Handle(RegisterSightingWithAssetRecordUpdatesCommand request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.StocktakeEventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(RegisterSightingWithAssetRecordUpdatesCommand), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.StocktakeEventId), true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(StocktakeEventErrors.EventNotFound(request.StocktakeEventId));
        }

        Asset asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(RegisterSightingWithAssetRecordUpdatesCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result sighting = @event.AddSighting(
            asset.SerialNumber,
            request.AssetNumber,
            asset.ModelDescription,
            request.LocationCategory,
            request.LocationName,
            request.LocationCode,
            request.UserType,
            request.UserName,
            request.UserCode,
            request.Comment,
            _currentUserService.UserName,
            _dateTime.Now,
            DifferenceCategory.UpdatedEntry);

        if (sighting.IsFailure)
        {
            _logger
                .ForContext(nameof(RegisterSightingWithAssetRecordUpdatesCommand), request, true)
                .ForContext(nameof(Error), sighting.Error, true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(sighting.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
