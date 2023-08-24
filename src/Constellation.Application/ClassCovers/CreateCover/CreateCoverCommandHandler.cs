namespace Constellation.Application.ClassCovers.CreateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System;
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
        _logger = logger.ForContext<CreateCoverCommandHandler>();
    }

    public async Task<Result<ClassCover>> Handle(CreateCoverCommand request, CancellationToken cancellationToken)
    {
        Result<ClassCover> coverResult = ClassCover.Create(
            new ClassCoverId(Guid.NewGuid()),
            request.OfferingId,
            request.StartDate,
            request.EndDate,
            request.TeacherType,
            request.TeacherId);

        if (coverResult.IsFailure)
        {
            var result = Result.Failure<ClassCover>(DomainErrors.GroupTutorials.GroupTutorial.CouldNotCreateTutorial);

            _logger.Warning("Error: {@error}", result);

            return result;
        }

        _classCoverRepository.Insert(coverResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return coverResult.Value;
    }
}
