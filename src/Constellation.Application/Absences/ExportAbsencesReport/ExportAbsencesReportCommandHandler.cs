namespace Constellation.Application.Absences.ExportAbsencesReport;

using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAbsencesReportCommandHandler
    : ICommandHandler<ExportAbsencesReportCommand, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IExcelService _excelService;

    public ExportAbsencesReportCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        ICourseOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository,
        IExcelService excelService)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
        _excelService = excelService;
    }

    public async Task<Result<FileDto>> Handle(ExportAbsencesReportCommand request, CancellationToken cancellationToken)
    {
        List<FilteredAbsenceResponse> result = new();

        if (!request.StudentIds.Any() &&
            !request.OfferingCodes.Any() &&
            !request.Grades.Any() &&
            !request.SchoolCodes.Any())
            return Result.Failure<FileDto>(DomainErrors.Absences.Report.NoFilterSupplied);

        List<Student> students = new();

        if (request.StudentIds.Any())
            students.AddRange(await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken));

        students.AddRange(await _studentRepository
            .GetFilteredStudents(
                request.OfferingCodes,
        request.Grades,
        request.SchoolCodes,
        cancellationToken));
        students = students
            .Distinct()
            .ToList();

        List<Absence> absences = await _absenceRepository.GetForStudents(students.Select(student => student.StudentId).ToList(), cancellationToken);

        foreach (var absence in absences)
        {
            var student = students.First(student => student.StudentId == absence.StudentId);

            Name? studentName = student.GetName();

            if (studentName is null)
                continue;

            CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            string offeringName = offering?.Name;

            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            result.Add(new(
                studentName,
                school.Name,
                student.CurrentGrade,
                offeringName,
                absence.Id,
                absence.Explained,
                absence.Date,
                absence.Type,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                absence.Notifications.Count(),
                absence.Responses.Count()));
        }

        var stream = await _excelService.CreateAbsencesReportFile(result, cancellationToken);

        var file = new FileDto
        {
            FileData = stream.ToArray(),
            FileName = "Absences Report.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        return file;
    }
}
