namespace Constellation.Application.Awards.ExportAwardNominations;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Extensions;
using Core.Models.Students.Identifiers;
using Core.Shared;
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

        List<AwardNominationExportDto> exportDtos = new();

        IEnumerable<IGrouping<StudentId, Nomination>> groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (IGrouping<StudentId, Nomination> student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);

            SchoolEnrolment? enrolment = studentEntry.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<string> awardDescriptions = student.Select(entry => entry.GetDescription()).ToList();
            var countOfAwardDescriptions = awardDescriptions.Select(entry => new { Name = entry, Count = awardDescriptions.Count(value => value == entry) });
            countOfAwardDescriptions = countOfAwardDescriptions.Distinct();
            string awards = string.Join("; ", countOfAwardDescriptions.Select(entry => entry.Count > 1 ? $"{entry.Name} x{entry.Count}" : entry.Name));

            exportDtos.Add(new(
                studentEntry.StudentReferenceNumber.Number,
                studentEntry.Name.FirstName,
                studentEntry.Name.LastName,
                studentEntry.Name.DisplayName,
                enrolment.Grade.AsName(),
                enrolment.SchoolName,
                awards));
        }

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFile(exportDtos, cancellationToken);

        var file = new FileDto
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
