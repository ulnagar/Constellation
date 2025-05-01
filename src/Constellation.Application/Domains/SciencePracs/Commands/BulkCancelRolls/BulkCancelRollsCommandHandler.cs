namespace Constellation.Application.Domains.SciencePracs.Commands.BulkCancelRolls;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.SciencePracs;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class BulkCancelRollsCommandHandler
: ICommandHandler<BulkCancelRollsCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public BulkCancelRollsCommandHandler(
        ILessonRepository lessonRepository,
        IOfferingRepository offeringRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<BulkCancelRollsCommand>();
    }

    public async Task<Result> Handle(BulkCancelRollsCommand request, CancellationToken cancellationToken)
    {
        string comment = $"{request.Comment}{Environment.NewLine}Roll cancelled by {_currentUserService.UserName} at {_dateTime.Now}";

        List<OfferingId> offeringIds = new();

        foreach (Grade grade in request.Grades)
        {
            List<Offering> offerings = await _offeringRepository.GetActiveByGrade(grade, cancellationToken);

            offeringIds.AddRange(offerings.Select(entry => entry.Id));
        }

        foreach (string code in request.SchoolCodes)
        {
            List<SciencePracLesson> lessons = await _lessonRepository.GetAllForSchool(code, cancellationToken);

            if (offeringIds.Any())
            {
                lessons = lessons
                    .Where(entry => 
                        entry.Offerings.Any(record => 
                            offeringIds.Contains(record.OfferingId)))
                    .ToList();
            }

            foreach (SciencePracLesson lesson in lessons)
            {
                SciencePracRoll roll = lesson.Rolls.FirstOrDefault(roll => roll.SchoolCode == code);

                if (roll is null || roll.Status != LessonStatus.Active)
                    continue;

                _logger.Information("Cancelling Roll for lesson {Lesson} for school {School}", lesson.Name, code);

                roll.CancelRoll(comment);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
