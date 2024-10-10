namespace Constellation.Application.Awards.ExportAwardNominations;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Helpers;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAwardNominationsCommandHandler
    : ICommandHandler<ExportAwardNominationsCommand, FileDto>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportAwardNominationsCommandHandler(
        IAwardNominationRepository nominationRepository,
        IStudentRepository studentRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _studentRepository = studentRepository;
        _excelService = excelService;
        _logger = logger.ForContext<ExportAwardNominationsCommand>();
    }

    public async Task<Result<FileDto>> Handle(ExportAwardNominationsCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _nominationRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger.Warning("Could not find Award Nomination Period with Id {id}", request.PeriodId);

            return Result.Failure<FileDto>(DomainErrors.Awards.NominationPeriod.NotFound(request.PeriodId));
        }

        return request.Category switch
        {
            _ when request.Category == ExportAwardNominationsCommand.GroupCategory.ByStudent =>
                await GroupByStudent(period, request.ShowGrade, request.ShowClass, cancellationToken),
            _ when request.Category == ExportAwardNominationsCommand.GroupCategory.BySchool =>
                await GroupBySchool(period, request.ShowGrade, request.ShowClass, cancellationToken),
            _ when request.Category == ExportAwardNominationsCommand.GroupCategory.BySubject =>
                await GroupBySubject(period, request.ShowGrade, request.ShowClass, cancellationToken),
            _ when request.Category == ExportAwardNominationsCommand.GroupCategory.None =>
                await NoGrouping(period, request.ShowGrade, request.ShowClass, cancellationToken),
            _ => Result.Failure<FileDto>(DomainErrors.Awards.NominationPeriod.NotFound(request.PeriodId))
        };
    }

    private async Task<FileDto> GroupByStudent(NominationPeriod period, bool showGrade, bool showClass, CancellationToken cancellationToken)
    {
        List<AwardNominationExportByStudentDto> exportDtos = new();

        IEnumerable<IGrouping<StudentId, Nomination>> groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (IGrouping<StudentId, Nomination> student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);

            if (studentEntry is null)
                continue;

            SchoolEnrolment? enrolment = studentEntry.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<string> awardDescriptions = student.Select(entry => entry.GetDescription(showGrade, showClass)).ToList();
            
            exportDtos.Add(new(
                studentEntry.StudentReferenceNumber,
                studentEntry.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                awardDescriptions));
        }

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFileByStudent(exportDtos, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }

    private async Task<FileDto> GroupBySchool(NominationPeriod period, bool showGrade, bool showClass, CancellationToken cancellationToken)
    {
        List<AwardNominationExportBySchoolDto> exportDtos = new();
        List<AwardNominationExportByStudentDto> studentDtos = new();

        IEnumerable<IGrouping<StudentId, Nomination>> groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (IGrouping<StudentId, Nomination> student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);

            if (studentEntry is null)
                continue;

            SchoolEnrolment? enrolment = studentEntry.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<string> awardDescriptions = student.Select(entry => entry.GetDescription(showGrade, showClass)).ToList();

            studentDtos.Add(new(
                studentEntry.StudentReferenceNumber,
                studentEntry.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                awardDescriptions));
        }

        IEnumerable<IGrouping<string, AwardNominationExportByStudentDto>> groupedDtos = studentDtos
            .OrderBy(entry => entry.Grade)
            .ThenBy(entry => entry.StudentName.SortOrder)
            .GroupBy(entry => entry.School);

        foreach (IGrouping<string, AwardNominationExportByStudentDto> school in groupedDtos.OrderBy(entry => entry.Key))
        {
            exportDtos.Add(new(
                school.First().School,
                school.ToList()));
        }

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFileBySchool(exportDtos, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }

    private async Task<FileDto> GroupBySubject(NominationPeriod period, bool showGrade, bool showClass, CancellationToken cancellationToken)
    {
        List<AwardNominationExportBySubjectDto> exportDtos = new();

        IEnumerable<IGrouping<StudentId, Nomination>> groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (IGrouping<StudentId, Nomination> student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);

            if (studentEntry is null)
                continue;

            SchoolEnrolment? enrolment = studentEntry.CurrentEnrolment;

            if (enrolment is null)
                continue;

            foreach (Nomination nomination in student)
            {
                string subjectName = nomination switch
                {
                    AcademicAchievementNomination typedNomination => typedNomination.CourseName,
                    AcademicAchievementMathematicsNomination typedNomination => typedNomination.CourseName,
                    AcademicAchievementScienceTechnologyNomination typedNomination => typedNomination.CourseName,
                    AcademicExcellenceNomination typedNomination => typedNomination.CourseName,
                    AcademicExcellenceMathematicsNomination typedNomination => typedNomination.CourseName,
                    AcademicExcellenceScienceTechnologyNomination typedNomination => typedNomination.CourseName,
                    FirstInSubjectNomination typedNomination => typedNomination.CourseName,
                    _ => nomination.GetDescription()
                };

                AwardNominationExportBySubjectDto subjectDto = exportDtos.FirstOrDefault(entry => entry.Subject == subjectName);

                if (subjectDto is null)
                {
                    subjectDto = new AwardNominationExportBySubjectDto(subjectName);
                    exportDtos.Add(subjectDto);
                }

                AwardNominationExportByStudentDto studentDto = subjectDto.Students.FirstOrDefault(entry => entry.SRN == studentEntry.StudentReferenceNumber);

                if (studentDto is null)
                {
                    studentDto = new AwardNominationExportByStudentDto(
                        studentEntry.StudentReferenceNumber,
                        studentEntry.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        new());

                    subjectDto.Students.Add(studentDto);
                }

                studentDto.Awards.Add(nomination.GetDescription(showGrade, showClass));
            }
        }

        foreach (var subjectDto in exportDtos)
        {
            subjectDto.Students = subjectDto.Students
                .OrderBy(entry => entry.Grade)
                .ThenBy(entry => entry.StudentName.SortOrder)
                .ToList();
        }

        exportDtos = exportDtos.OrderBy(entry => entry.Subject).ToList();

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFileBySubject(exportDtos, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }

    private async Task<FileDto> NoGrouping(NominationPeriod period, bool showGrade, bool showClass, CancellationToken cancellationToken)
    {
        List<AwardNominationExportDto> exportDtos = new();

        IEnumerable<IGrouping<StudentId, Nomination>> groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (IGrouping<StudentId, Nomination> student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);

            if (studentEntry is null)
                continue;

            SchoolEnrolment? enrolment = studentEntry.CurrentEnrolment;

            if (enrolment is null)
                continue;

            foreach (var nomination in student)
            {
                exportDtos.Add(new(
                    studentEntry.StudentReferenceNumber,
                    studentEntry.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    nomination.GetDescription(showGrade, showClass)));
            }
        }

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFile(exportDtos, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
