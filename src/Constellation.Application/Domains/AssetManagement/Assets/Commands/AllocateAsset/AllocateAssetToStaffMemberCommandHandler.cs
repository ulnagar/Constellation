namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AllocateAssetToStaffMemberCommandHandler 
: ICommandHandler<AllocateAssetToStaffMemberCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AllocateAssetToStaffMemberCommandHandler(
        IAssetRepository assetRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<AllocateAssetToStaffMemberCommand>();
    }

    public async Task<Result> Handle(AllocateAssetToStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStaffMemberCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to allocate device to staff member");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStaffMemberCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to allocate device to staff member");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        Result<Allocation> allocation = Allocation.Create(asset.Id, staffMember, _dateTime.Today);

        if (allocation.IsFailure)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStaffMemberCommand), request, true)
                .ForContext(nameof(Error), allocation.Error, true)
                .Warning("Failed to allocate device to staff member");

            return Result.Failure(allocation.Error);
        }

        asset.AddAllocation(allocation.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
