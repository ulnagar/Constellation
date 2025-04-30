namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportUnexplainedPartialAbsencesReport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Absences;
using Core.Models.Offerings;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using Core.Shared;
using DTOs;
using Helpers;
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
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportUnexplainedPartialAbsencesReportCommandHandler(
        IAbsenceRepository absenceRepository, 
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _excelService = excelService;
        _logger = logger.ForContext<ExportUnexplainedPartialAbsencesReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(ExportUnexplainedPartialAbsencesReportCommand request, CancellationToken cancellationToken)
    {
        List<UnexplainedPartialAbsenceResponse> responses = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<Absence> absences = await _absenceRepository.GetUnexplainedPartialAbsences(cancellationToken);

        IEnumerable<IGrouping<StudentId, Absence>> groupedAbsences = absences.GroupBy(absence => absence.StudentId);

        foreach (IGrouping<StudentId, Absence> absenceGroup in groupedAbsences)
        {
            Student student = students.FirstOrDefault(student => student.Id == absenceGroup.Key);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(ExportUnexplainedPartialAbsencesReportCommand), request, true)
                    .ForContext(nameof(Absence), absenceGroup.First(), true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(absenceGroup.Key), true);

                continue;
            }

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;
            
            foreach (Absence absence in absenceGroup)
            {
                Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                string offeringName = offering?.Name;

                Response response = absence.Responses.FirstOrDefault(response => response.Type.Equals(ResponseType.Student));

                responses.Add(new(
                    absence.Id,
                    student.Id,
                    student.Name.FirstName,
                    student.Name.LastName,
                    enrolment.Grade,
                    enrolment.SchoolName,
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
