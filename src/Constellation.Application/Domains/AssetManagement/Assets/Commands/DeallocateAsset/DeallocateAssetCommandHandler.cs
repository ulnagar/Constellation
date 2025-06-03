namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.DeallocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeallocateAssetCommandHandler
: ICommandHandler<DeallocateAssetCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public DeallocateAssetCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<DeallocateAssetCommand>();
    }

    public async Task<Result> Handle(DeallocateAssetCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(DeallocateAssetCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Could not deallocate device");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result result = asset.Deallocate(_dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(DeallocateAssetCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Could not deallocate device");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
