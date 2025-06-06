﻿namespace Constellation.Application.Domains.SciencePracs.Commands.SubmitRoll;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitRollCommandHandler
    : ICommandHandler<SubmitRollCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SubmitRollCommandHandler(
        ICurrentUserService currentUserService,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SubmitRollCommand>();
    }

    public async Task<Result> Handle(SubmitRollCommand request, CancellationToken cancellationToken) 
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", request.RollId);

            return Result.Failure(SciencePracRollErrors.NotFound(request.RollId));
        }

        Result submitRequest = lesson.MarkRoll(
            roll.Id,
            _currentUserService.EmailAddress,
            DateOnly.FromDateTime(request.LessonDate),
            request.Comment,
            request.PresentStudents,
            request.AbsentStudents);

        if (submitRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(submitRequest.Error), submitRequest.Error, true)
                .Warning("Could not mark roll due to error");

            return Result.Failure(submitRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
