namespace Constellation.Application.Domains.ClassCovers.Commands.CreateCover;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Covers;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public class CreateCoverCommandHandler : ICommandHandler<CreateCoverCommand, ClassCover>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCoverCommandHandler(IClassCoverRepository classCoverRepository, IUnitOfWork unitOfWork, Serilog.ILogger logger)
    {
        _classCoverRepository = classCoverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateCoverCommandHandler>();
    }

    public async Task<Result<ClassCover>> Handle(CreateCoverCommand request, CancellationToken cancellationToken)
    {
        Result<ClassCover> coverResult = ClassCover.Create(
            new(),
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

            return Result.Failure<ClassCover>(DomainErrors.GroupTutorials.GroupTutorial.CouldNotCreateTutorial);
        }

        _classCoverRepository.Insert(coverResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return coverResult.Value;
    }
}
