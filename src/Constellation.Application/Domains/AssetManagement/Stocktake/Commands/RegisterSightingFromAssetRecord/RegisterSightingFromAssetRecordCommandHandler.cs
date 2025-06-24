namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingFromAssetRecord;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Extensions;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterSightingFromAssetRecordCommandHandler
    : ICommandHandler<RegisterSightingFromAssetRecordCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RegisterSightingFromAssetRecordCommandHandler(
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
            .ForContext<RegisterSightingFromAssetRecordCommand>();
    }
    public async Task<Result> Handle(RegisterSightingFromAssetRecordCommand request, CancellationToken cancellationToken)
    {
        Asset asset = await _assetRepository.GetById(request.AssetId, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(RegisterSightingFromAssetRecordCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundById(request.AssetId), true)
                .Warning("Failed to register asset sighting for stocktake by user {User}", _currentUserService.UserName);

            return Result.Failure(AssetErrors.NotFoundById(request.AssetId));
        }

        StocktakeEvent @event = await _stocktakeRepository.GetById(request.EventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(RegisterSightingFromAssetRecordCommand), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.EventId), true)
                .Warning("Failed to register asset sighting for stocktake by user {User}", _currentUserService.UserName);

            return Result.Failure(StocktakeEventErrors.EventNotFound(request.EventId));
        }

        Result sighting = @event.AddSighting(
            asset.SerialNumber,
            asset.AssetNumber,
            asset.ModelDescription,
            asset.CurrentLocation?.Category.AsStocktakeLocationCategory() ?? LocationCategory.Other,
            asset.CurrentLocation?.Site ?? string.Empty,
            asset.CurrentLocation?.SchoolCode ?? string.Empty,
            asset.CurrentAllocation?.AllocationType.AsStocktakeUserType() ?? UserType.Other,
            asset.CurrentAllocation?.ResponsibleOfficer ?? string.Empty,
            asset.CurrentAllocation?.UserId ?? string.Empty,
            request.Comment,
            _currentUserService.UserName,
            _dateTime.Now);

        if (sighting.IsFailure)
        {
            _logger
                .ForContext(nameof(RegisterSightingFromAssetRecordCommand), request, true)
                .ForContext(nameof(Error), sighting.Error, true)
                .Warning("Failed to register asset sighting for stocktake by user {User}", _currentUserService.UserName);

            return Result.Failure(sighting.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
