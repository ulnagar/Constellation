namespace Constellation.Infrastructure.Jobs;

using Application.Domains.SciencePracs.Commands.UpdateLessonGrade;
using Application.Interfaces.Jobs;
using Constellation.Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.SciencePracs;
using System;
using System.Threading.Tasks;


internal sealed class LessonUpdateJob : ILessonUpdateJob
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISender _mediator;

    public LessonUpdateJob(
        ILessonRepository lessonRepository,
        ISender mediator)
    {
        _lessonRepository = lessonRepository;
        _mediator = mediator;
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<SciencePracLesson> lessons = await _lessonRepository.GetAll(cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            if (lesson.Grade != Grade.SpecialProgram)
                continue;

            await _mediator.Send(new UpdateLessonGradeCommand(lesson.Id), cancellationToken);
        }
    }
}