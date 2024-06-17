namespace Constellation.Application.Assets.AddAssetNote;

using Abstractions.Messaging;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddAssetNoteCommandHandler
: ICommandHandler<AddAssetNoteCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddAssetNoteCommandHandler(
        IAssetRepository assetRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddAssetNoteCommand>();
    }

    public async Task<Result> Handle(AddAssetNoteCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(AddAssetNoteCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to add asset note");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Result<Note> note = Note.Create(
            asset.Id,
            request.Note);

        if (note.IsFailure)
        {
            _logger
                .ForContext(nameof(AddAssetNoteCommand), request, true)
                .ForContext(nameof(Error), note.Error, true)
                .Warning("Failed to add asset note");

            return Result.Failure(note.Error);
        }

        asset.AddNote(note.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
