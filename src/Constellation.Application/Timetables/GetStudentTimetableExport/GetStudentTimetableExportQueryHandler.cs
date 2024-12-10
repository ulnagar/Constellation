namespace Constellation.Application.Timetables.GetStudentTimetableExport;

using Abstractions.Messaging;
using Core.Extensions;
using Core.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Models.Timetables.ValueObjects;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentTimetableExportQueryHandler
: IQueryHandler<GetStudentTimetableExportQuery, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;
    private readonly ILogger _logger;

    public GetStudentTimetableExportQueryHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IRazorViewToStringRenderer renderService, 
        IPDFService pdfService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _renderService = renderService;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<Result<FileDto>> Handle(GetStudentTimetableExportQuery request, CancellationToken cancellationToken)
    {
        StudentTimetableDataDto response = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableExportQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<FileDto>(StudentErrors.NotFound(request.StudentId));
        }

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableExportQuery), request, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<FileDto>(SchoolEnrolmentErrors.NotFound);
        }

        response.StudentId = student.Id;
        response.StudentName = student.Name.DisplayName;
        response.StudentGrade = enrolment.Grade.AsName();
        response.StudentSchool = enrolment.SchoolName;

        List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableExportQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent(student.Id), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<FileDto>(OfferingErrors.NotFoundForStudent(student.Id));
        }

        List<PeriodId> periodIds = offerings
            .SelectMany(offering => offering.Sessions)
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .Distinct()
            .ToList();

        List<Period> periods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

        if (periods.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableExportQuery), request, true)
                .ForContext(nameof(Error), PeriodErrors.NoneFoundForOffering, true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<FileDto>(PeriodErrors.NoneFoundForOffering);
        }

        List<Timetable> relevantTimetables = periods
            .Select(period => period.Timetable)
            .Distinct()
            .ToList();

        List<Period> relevantPeriods = await _periodRepository.GetAllFromTimetable(relevantTimetables, cancellationToken);

        foreach (Period period in relevantPeriods)
        {
            if (period.Type == PeriodType.Offline)
                continue;

            TimetableDataDto.TimetableData entry = new()
            {
                Timetable = period.Timetable,
                Week = period.Week,
                Day = period.Day,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                Name = period.Name,
                PeriodCode = period.PeriodCode,
                Type = period.Type
            };

            if (periodIds.Contains(period.Id))
            {
                Offering offering = offerings
                    .FirstOrDefault(offering =>
                        offering.Sessions.Any(session =>
                            !session.IsDeleted &&
                            session.PeriodId == period.Id));

                if (offering is null)
                    continue;

                entry.ClassName = offering.Name;

                List<TeacherAssignment> assignments = offering
                    .Teachers
                    .Where(assignment =>
                        assignment.Type == AssignmentType.ClassroomTeacher &&
                        !assignment.IsDeleted)
                    .ToList();

                foreach (TeacherAssignment assignment in assignments)
                {
                    Staff teacher = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

                    if (teacher is null)
                        continue;

                    entry.ClassTeacher = teacher.DisplayName;
                }
            }

            response.Timetables.Add(entry);
        }

        string fileName = $"{response.StudentName} Timetable.pdf";

        string headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/StudentTimetableHeader.cshtml", response);
        string htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", response);

        MemoryStream pdfStream = _pdfService.StringToPdfStream(htmlString, headerString);

        FileDto result = new()
        {
            FileName = fileName,
            FileData = pdfStream.ToArray(),
            FileType = MediaTypeNames.Application.Pdf
        };

        return result;
    }
}
