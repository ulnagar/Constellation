namespace Constellation.Application.Domains.Covers.Commands.CreateCover;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Covers.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public class CreateCoverCommandHandler : ICommandHandler<CreateCoverCommand, Cover>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCoverCommandHandler(ICoverRepository coverRepository, IUnitOfWork unitOfWork, Serilog.ILogger logger)
    {
        _coverRepository = coverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateCoverCommandHandler>();
    }

    public async Task<Result<Cover>> Handle(CreateCoverCommand request, CancellationToken cancellationToken)
    {
        if (request.CoverType.Equals(CoverType.ClassCover))
        {
            Result<ClassCover> coverResult = ClassCover.Create(
                request.OfferingId,
                request.StartDate,
                request.EndDate,
                request.TeacherType,
                request.TeacherId);

            if (coverResult.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateCoverCommand), request, true)
                    .ForContext(nameof(Error), coverResult.Error, true)
                    .Warning("Failed to create Class Cover");

                return Result.Failure<Cover>(coverResult.Error);
            }

            _coverRepository.Insert(coverResult.Value);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return coverResult.Value;
        }

        if (request.CoverType.Equals(CoverType.AccessCover))
        {
            Result<AccessCover> coverResult = AccessCover.Create(
                request.OfferingId,
                request.StartDate,
                request.EndDate,
                request.TeacherType,
                request.TeacherId,
                request.Note);

            if (coverResult.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateCoverCommand), request, true)
                    .ForContext(nameof(Error), coverResult.Error, true)
                    .Warning("Failed to create Access Cover");

                return Result.Failure<Cover>(coverResult.Error);
            }

            _coverRepository.Insert(coverResult.Value);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return coverResult.Value;
        }

        return Result.Failure<Cover>(CoverErrors.CouldNotDetermineCoverType);
    }
}
