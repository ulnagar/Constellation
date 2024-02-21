namespace Constellation.Application.Timetables.GetStudentTimetableData;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Extensions;
using Core.Models.Offerings.Errors;
using Core.Models.Students.Errors;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentTimetableDataQueryHandler 
    : IQueryHandler<GetStudentTimetableDataQuery, StudentTimetableDataDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStudentTimetableDataQueryHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IOfferingRepository offeringRepository,
        ITimetablePeriodRepository periodRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetStudentTimetableDataQuery>();
    }

    public async Task<Result<StudentTimetableDataDto>> Handle(GetStudentTimetableDataQuery request, CancellationToken cancellationToken)
    {
        StudentTimetableDataDto response = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(StudentErrors.NotFound(request.StudentId));
        }

        School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(student.SchoolCode), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(DomainErrors.Partners.School.NotFound(student.SchoolCode));
        }

        response.StudentId = student.StudentId;
        response.StudentName = student.GetName().DisplayName;
        response.StudentGrade = student.CurrentGrade.AsName();
        response.StudentSchool = school.Name;

        List<Offering> offerings = await _offeringRepository.GetByStudentId(student.StudentId, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent(student.StudentId), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(OfferingErrors.NotFoundForStudent(student.StudentId));
        }

        List<int> periodIds = offerings
            .SelectMany(offering => offering.Sessions)
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .Distinct()
            .ToList();

        List<TimetablePeriod> periods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

        if (periods.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Period.NoneFoundForOffering, true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(DomainErrors.Period.NoneFoundForOffering);
        }

        List<string> relevantTimetables = periods
            .Select(period => period.Timetable)
            .Distinct()
            .ToList();

        List<TimetablePeriod> relevantPeriods = await _periodRepository.GetAllFromTimetable(relevantTimetables, cancellationToken);

        foreach (TimetablePeriod period in relevantPeriods)
        {
            if (period.Type == "Other")
                continue;

            TimetableDataDto.TimetableData entry = new()
            {
                Day = period.Day,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                TimetableName = period.Timetable,
                Name = period.Name,
                Period = period.Period,
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

        return response;
    }
}
