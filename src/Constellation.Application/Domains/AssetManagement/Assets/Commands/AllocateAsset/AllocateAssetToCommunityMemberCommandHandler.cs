namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AllocateAssetToCommunityMemberCommandHandler
: ICommandHandler<AllocateAssetToCommunityMemberCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AllocateAssetToCommunityMemberCommandHandler(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<AllocateAssetToCommunityMemberCommand>();
    }

    public async Task<Result> Handle(AllocateAssetToCommunityMemberCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToCommunityMemberCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to allocate device to community member");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result<EmailRecipient> recipient = EmailRecipient.Create(request.UserName, request.UserEmail);

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(AllocateAssetToCommunityMemberCommand), request, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to allocate device to community member");

            return Result.Failure(recipient.Error);
        }

        Result<Allocation> allocation = Allocation.Create(asset.Id, recipient.Value, _dateTime.Today);

        if (allocation.IsFailure)
        {
            _logger
                .ForContext(nameof(AllocateAssetToCommunityMemberCommand), request, true)
                .ForContext(nameof(Error), allocation.Error, true)
                .Warning("Failed to allocate device to community member");

            return Result.Failure(allocation.Error);
        }

        asset.AddAllocation(allocation.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
