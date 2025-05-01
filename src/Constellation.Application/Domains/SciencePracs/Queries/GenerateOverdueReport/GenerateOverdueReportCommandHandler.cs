namespace Constellation.Application.Domains.SciencePracs.Queries.GenerateOverdueReport;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.SciencePracs;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using DTOs;
using Helpers;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateOverdueReportCommandHandler
: ICommandHandler<GenerateOverdueReportCommand, FileDto>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateOverdueReportCommandHandler(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        ICourseRepository courseRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _courseRepository = courseRepository;
        _excelService = excelService;
        _logger = logger.ForContext<GenerateOverdueReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(GenerateOverdueReportCommand request, CancellationToken cancellationToken)
    {
        List<OverdueRollResponse> responses = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllWithOverdueRolls(cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            IEnumerable<SciencePracRoll> overdueRolls = lesson.Rolls.Where(roll => roll.Status == LessonStatus.Active);

            foreach (SciencePracRoll roll in overdueRolls)
            {
                School school = await _schoolRepository.GetById(roll.SchoolCode, cancellationToken);

                if (school is null)
                {
                    //TODO: Properly log error

                    continue;
                }

                Course course = await _courseRepository.GetByOfferingId(lesson.Offerings.First().OfferingId, cancellationToken);

                if (course is null)
                {
                    //TODO: Properly log error

                    continue;
                }

                responses.Add(new(
                    school.Name,
                    lesson.Name,
                    course.ToString(),
                    lesson.DueDate.ToDateTime(TimeOnly.MinValue)));
            }
        }

        MemoryStream reportStream = await _excelService.CreateSciencePracOverdueReport(responses, cancellationToken);

        FileDto file = new()
        {
            FileData = reportStream.ToArray(),
            FileName = $"Science Prac Overdue Report.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}