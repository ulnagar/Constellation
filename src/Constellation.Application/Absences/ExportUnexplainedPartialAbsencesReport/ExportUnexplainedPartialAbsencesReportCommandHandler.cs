namespace Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.Absences;
using Core.Models.Offerings;
using Core.Models.Students.Errors;
using Core.Shared;
using Core.ValueObjects;
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

internal sealed class ExportUnexplainedPartialAbsencesReportCommandHandler
: ICommandHandler<ExportUnexplainedPartialAbsencesReportCommand, FileDto>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportUnexplainedPartialAbsencesReportCommandHandler(
        IAbsenceRepository absenceRepository, 
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
        _excelService = excelService;
        _logger = logger.ForContext<ExportUnexplainedPartialAbsencesReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(ExportUnexplainedPartialAbsencesReportCommand request, CancellationToken cancellationToken)
    {
        List<UnexplainedPartialAbsenceResponse> responses = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        List<Absence> absences = await _absenceRepository.GetUnexplainedPartialAbsences(cancellationToken);

        IEnumerable<IGrouping<string, Absence>> groupedAbsences = absences.GroupBy(absence => absence.StudentId);

        foreach (IGrouping<string, Absence> absenceGroup in groupedAbsences)
        {
            Student student = students.FirstOrDefault(student => student.StudentId == absenceGroup.Key);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(ExportUnexplainedPartialAbsencesReportCommand), request, true)
                    .ForContext(nameof(Absence), absenceGroup.First(), true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(absenceGroup.Key), true);

                continue;
            }

            Name studentName = student.GetName();

            if (studentName is null)
                continue;

            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            foreach (Absence absence in absenceGroup)
            {
                Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                string offeringName = offering?.Name;

                Response response = absence.Responses.FirstOrDefault(response => response.Type.Equals(ResponseType.Student));

                responses.Add(new(
                    absence.Id,
                    student.StudentId,
                    studentName.FirstName,
                    studentName.LastName,
                    student.CurrentGrade,
                    school?.Name,
                    absence.Date,
                    offeringName,
                    absence.AbsenceLength,
                    absence.AbsenceTimeframe,
                    response is null ? "Pending Response" : "Pending Verification",
                    response is null ? null : DateOnly.FromDateTime(response.ReceivedAt)));
            }
        }

        MemoryStream stream = await _excelService.CreateUnexplainedPartialAbsencesReportFile(responses, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Absences Report.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
