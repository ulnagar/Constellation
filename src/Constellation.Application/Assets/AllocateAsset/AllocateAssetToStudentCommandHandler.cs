namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Models.Awards.Events;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AllocateAssetToStudentCommandHandler
: ICommandHandler<AllocateAssetToStudentCommand>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AllocateAssetToStudentCommandHandler(
        IAssetRepository assetRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<AllocateAssetToStudentCommand>();
    }

    public async Task<Result> Handle(AllocateAssetToStudentCommand request, CancellationToken cancellationToken)
    {
        Asset? asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStudentCommand), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to allocate device to student");

            return Result.Failure(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to allocate device to student");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Result<Allocation> allocation = Allocation.Create(asset.Id, student, _dateTime.Today);

        if (allocation.IsFailure)
        {
            _logger
                .ForContext(nameof(AllocateAssetToStudentCommand), request, true)
                .ForContext(nameof(Error), allocation.Error, true)
                .Warning("Failed to allocate device to student");

            return Result.Failure(allocation.Error);
        }

        asset.AddAllocation(allocation.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
