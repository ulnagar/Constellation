namespace Constellation.Application.ClassCovers.BulkCreateCovers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ClassCovers.CreateCover;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BulkCreateCoversCommandHandler : ICommandHandler<BulkCreateCoversCommand, List<ClassCover>>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public BulkCreateCoversCommandHandler(IClassCoverRepository classCoverRepository, IUnitOfWork unitOfWork, ILogger logger)
    {
        _classCoverRepository = classCoverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateCoverCommandHandler>();
    }

    public async Task<Result<List<ClassCover>>> Handle(BulkCreateCoversCommand request, CancellationToken cancellationToken)
    {
        List<Error> errors = new();
        List<ClassCover> covers = new();

        foreach (var offeringId in request.OfferingId)
        {
            Result<ClassCover> coverResult = ClassCover.Create(
                new ClassCoverId(Guid.NewGuid()),
                offeringId,
                request.StartDate,
                request.EndDate,
                request.TeacherType,
                request.TeacherId);

            if (coverResult.IsFailure)
            {
                var result = DomainErrors.GroupTutorials.GroupTutorial.CouldNotCreateTutorial;

                _logger.Warning("Error: {@error}", result);

                errors.Add(result);
            }

            covers.Add(coverResult.Value);
        }

        if (errors.Count > 0)
        {
            return Result.Failure<List<ClassCover>>(errors.First());
        }

        foreach (var cover in covers)
        {
            _classCoverRepository.Insert(cover);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success(covers);
    }
}
