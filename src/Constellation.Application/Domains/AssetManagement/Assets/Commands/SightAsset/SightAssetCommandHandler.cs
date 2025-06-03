namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.SightAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SightAssetCommandHandler
: ICommandHandler<SightAssetCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SightAssetCommandHandler(
        IAssetRepository assetRepository,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SightAssetCommand>();
    }

    public async Task<Result> Handle(SightAssetCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(SightAssetCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to register asset sighting");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        StaffMember sightedBy = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (sightedBy is null)
        {
            _logger
                .ForContext(nameof(SightAssetCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to register asset sighting");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        Result<Sighting> sighting = Sighting.Create(
            asset.Id,
            sightedBy.Name.DisplayName,
            request.SightedAt,
            request.Note,
            _dateTime);

        if (sighting.IsFailure)
        {
            _logger
                .ForContext(nameof(SightAssetCommand), request, true)
                .ForContext(nameof(Error), sighting.Error, true)
                .Warning("Failed to register asset sighting");

            return Result.Failure(sighting.Error);
        }

        Result result = asset.AddSighting(sighting.Value);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(SightAssetCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to register asset sighting");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
