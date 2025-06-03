namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AllocateAssetToSchoolCommandHandler
: ICommandHandler<AllocateAssetToSchoolCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AllocateAssetToSchoolCommandHandler(
        IAssetRepository assetRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<AllocateAssetToSchoolCommand>();
    }

    public async Task<Result> Handle(AllocateAssetToSchoolCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToSchoolCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to allocate device to school");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToSchoolCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to allocate device to school");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        Result<Allocation> allocation = Allocation.Create(asset.Id, school, _dateTime.Today);

        if (allocation.IsFailure)
        {
            _logger
                .ForContext(nameof(AllocateAssetToSchoolCommand), request, true)
                .ForContext(nameof(Error), allocation.Error, true)
                .Warning("Failed to allocate device to school");

            return Result.Failure(allocation.Error);
        }

        asset.AddAllocation(allocation.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
