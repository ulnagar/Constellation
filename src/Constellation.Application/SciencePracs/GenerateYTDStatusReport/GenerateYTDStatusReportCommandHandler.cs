namespace Constellation.Application.SciencePracs.GenerateYTDStatusReport;

using Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Models.SciencePracs;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


internal sealed class GenerateYTDStatusReportCommandHandler
: ICommandHandler<GenerateYTDStatusReportCommand, FileDto>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateYTDStatusReportCommandHandler(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        IStudentRepository studentRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _studentRepository = studentRepository;
        _excelService = excelService;
        _logger = logger
            .ForContext<GenerateYTDStatusReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(GenerateYTDStatusReportCommand request, CancellationToken cancellationToken)
    {
        List<RollStatusResponse> responses = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAll(cancellationToken);

        List<School> schools = await _schoolRepository.GetWithCurrentStudents(cancellationToken);

        foreach (School school in schools)
        {
            List<RollStatusResponse.RollStatusGradeData> gradeData = [];

            List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(school.Code, cancellationToken);

            foreach (Grade grade in Enum.GetValues<Grade>())
            {
                if (grade is Grade.Y05 or Grade.Y06)
                    continue;

                List<SciencePracRoll> rolls = lessons
                    .Where(lesson => lesson.Grade == grade)
                    .SelectMany(lesson => lesson.Rolls)
                    .Where(roll => roll.SchoolCode == school.Code)
                    .ToList();

                int submittedRolls = rolls.Count(roll => roll.Status == LessonStatus.Completed);
                int totalRolls = rolls.Count(roll => roll.Status != LessonStatus.Cancelled);

                gradeData.Add(new(
                    grade,
                    submittedRolls,
                    totalRolls,
                    students.Any(student => student.CurrentEnrolment?.Grade == grade)));
            }

            responses.Add(new(
                school.Code,
                school.Name,
                gradeData));
        }

        MemoryStream reportStream = await _excelService.CreateSciencePracYTDReport(responses, cancellationToken);

        FileDto file = new()
        {
            FileData = reportStream.ToArray(),
            FileName = $"Science Prac YTD Report.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}