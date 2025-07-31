#nullable enable
namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAbsencesReport;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.Absences.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using DTOs;
using Helpers;
using Interfaces.Repositories;
using Interfaces.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportAbsencesReportCommandHandler
    : ICommandHandler<ExportAbsencesReportCommand, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IExcelService _excelService;

    public ExportAbsencesReportCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ISchoolRepository schoolRepository,
        IExcelService excelService)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _schoolRepository = schoolRepository;
        _excelService = excelService;
    }

    public async Task<Result<FileDto>> Handle(ExportAbsencesReportCommand request, CancellationToken cancellationToken)
    {
        List<FilteredAbsenceResponse> result = [];

        if (request.StudentIds.Count == 0 &&
            request.OfferingCodes.Count == 0 &&
            request.Grades.Count == 0 &&
            request.SchoolCodes.Count == 0)
            return Result.Failure<FileDto>(DomainErrors.Absences.Report.NoFilterSupplied);

        List<Student> students = [];

        if (request.StudentIds.Count > 0)
            students.AddRange(await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken));

        if (request.OfferingCodes.Count > 0 || request.Grades.Count > 0 || request.SchoolCodes.Count > 0)
            students.AddRange(await _studentRepository
                .GetFilteredStudents(
                    request.OfferingCodes,
                    request.Grades,
                    request.SchoolCodes,
                    cancellationToken));

        students = students
            .Distinct()
            .ToList();
        
        List<Absence> absences = await _absenceRepository.GetForStudents(students.Select(student => student.Id).ToList(), cancellationToken);

        foreach (Absence absence in absences)
        {
            Student student = students.First(student => student.Id == absence.StudentId);

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering? offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                if (offering is null)
                    continue;

                activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial? tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                if (tutorial is null)
                    continue;

                activityName = tutorial.Name;
            }

            School? school = await _schoolRepository.GetById(enrolment.SchoolCode, cancellationToken);

            string schoolName = school?.Name ?? string.Empty;

            result.Add(new(
                student.Name,
                schoolName,
                enrolment.Grade,
                activityName,
                absence.Id,
                absence.Explained,
                absence.Date,
                absence.Type,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                absence.Notifications.Count,
                absence.Responses.Count));
        }

        MemoryStream stream = await _excelService.CreateAbsencesReportFile(result, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Absences Report.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
