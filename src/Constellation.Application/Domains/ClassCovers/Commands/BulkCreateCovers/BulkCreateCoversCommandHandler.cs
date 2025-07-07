namespace Constellation.Application.Domains.ClassCovers.Commands.BulkCreateCovers;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Covers;
using Core.Models.Identifiers;
using Core.Shared;
using CreateCover;
using Interfaces.Repositories;
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
                new(),
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
