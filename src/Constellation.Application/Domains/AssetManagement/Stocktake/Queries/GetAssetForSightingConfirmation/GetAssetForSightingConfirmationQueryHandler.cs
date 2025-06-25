namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Stocktake.Enums;
using Core.Extensions;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssetForSightingConfirmationQueryHandler
: IQueryHandler<GetAssetForSightingConfirmationQuery, AssetSightingResponse>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetAssetForSightingConfirmationQueryHandler(
        IAssetRepository assetRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<AssetSightingResponse>> Handle(GetAssetForSightingConfirmationQuery request, CancellationToken cancellationToken)
    {
        Asset asset = (request.AssetNumber != AssetNumber.Empty)
            ? await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken)
            : await _assetRepository.GetBySerialNumber(request.SerialNumber, cancellationToken);

        if (asset is null)
        {
            Error error = (request.AssetNumber != AssetNumber.Empty)
                ? AssetErrors.NotFoundByAssetNumber(request.AssetNumber)
                : AssetErrors.NotFoundBySerialNumber(request.SerialNumber);

            _logger
                .ForContext(nameof(GetAssetForSightingConfirmationQuery), request, true)
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to retrieve asset for stocktake sighting by user {User}", _currentUserService.UserName);

            return Result.Failure<AssetSightingResponse>(error);
        }
        
        AssetSightingResponse response = new(
            asset.Id,
            asset.SerialNumber,
            asset.AssetNumber,
            asset.ModelDescription,
            asset.CurrentLocation?.Category.AsStocktakeLocationCategory() ?? LocationCategory.Other,
            asset.CurrentLocation?.Site ?? string.Empty,
            asset.CurrentLocation?.SchoolCode ?? string.Empty,
            asset.CurrentAllocation?.AllocationType.AsStocktakeUserType() ?? UserType.Other,
            asset.CurrentAllocation?.ResponsibleOfficer ?? string.Empty,
            asset.CurrentAllocation?.UserId ?? string.Empty);

        return response;
    }
}
