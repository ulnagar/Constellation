﻿namespace Constellation.Application.Domains.ClassCovers.Commands.CreateCover;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Covers;
using Core.Models.Identifiers;
using Core.Shared;
using Interfaces.Repositories;
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
