namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.TransferAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class TransferAssetToPublicSchoolCommandHandler
: ICommandHandler<TransferAssetToPublicSchoolCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public TransferAssetToPublicSchoolCommandHandler(
        IAssetRepository assetRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<TransferAssetToPublicSchoolCommand>();
    }

    public async Task<Result> Handle(TransferAssetToPublicSchoolCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to transfer asset to Public School");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        School? school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to transfer asset to Public School");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }
        
        Result<Location> location = Location.CreatePublicSchoolLocationRecord(
            asset.Id,
            school.Name,
            school.Code,
            request.CurrentLocation,
            request.ArrivalDate);

        if (location.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), request, true)
                .ForContext(nameof(Error), location.Error, true)
                .Warning("Failed to transfer asset to Public School");

            return Result.Failure(location.Error);
        }

        Result result = asset.AddLocation(location.Value);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to transfer asset to Public School");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
