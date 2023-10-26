namespace Constellation.Application.Awards.ExportAwardNominations;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAwardNominationsCommandHandler
    : ICommandHandler<ExportAwardNominationsCommand, FileDto>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportAwardNominationsCommandHandler(
        IAwardNominationRepository nominationRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
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

        var groupedNominations = period.Nominations.Where(nomination => !nomination.IsDeleted).GroupBy(nomination => nomination.StudentId);

        foreach (var student in groupedNominations)
        {
            Student studentEntry = await _studentRepository.GetById(student.Key, cancellationToken);
            School school = await _schoolRepository.GetById(studentEntry.SchoolCode, cancellationToken);

            List<string> awardDescriptions = student.Select(entry => entry.GetDescription()).ToList();
            var countOfAwardDescriptions = awardDescriptions.Select(entry => new { Name = entry, Count = awardDescriptions.Count(value => value == entry) });
            countOfAwardDescriptions = countOfAwardDescriptions.Distinct();
            string awards = string.Join("; ", countOfAwardDescriptions.Select(entry => entry.Count > 1 ? $"{entry.Name} x{entry.Count}" : entry.Name));

            exportDtos.Add(new(
                studentEntry.StudentId,
                studentEntry.FirstName,
                studentEntry.LastName,
                studentEntry.DisplayName,
                studentEntry.CurrentGrade.AsName(),
                school.Name,
                awards));
        }

        MemoryStream stream = await _excelService.CreateAwardNominationsExportFile(exportDtos, cancellationToken);

        var file = new FileDto
        {
            FileData = stream.ToArray(),
            FileName = "Award Nominations.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        return file;
    }
}
