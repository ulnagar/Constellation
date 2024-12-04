namespace Constellation.Application.Timetables.GetStudentTimetableData;

using Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Extensions;
using Core.Models.Offerings.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Errors;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Models.Timetables.ValueObjects;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentTimetableDataQueryHandler 
    : IQueryHandler<GetStudentTimetableDataQuery, StudentTimetableDataDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStudentTimetableDataQueryHandler(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
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

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(SchoolEnrolmentErrors.NotFound);
        }

        response.StudentId = student.Id;
        response.StudentName = student.Name.DisplayName;
        response.StudentGrade = enrolment.Grade.AsName();
        response.StudentSchool = enrolment.SchoolName;

        List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent(student.Id), true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(OfferingErrors.NotFoundForStudent(student.Id));
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
                .ForContext(nameof(GetStudentTimetableDataQuery), request, true)
                .ForContext(nameof(Error), PeriodErrors.NoneFoundForOffering, true)
                .Warning("Failed to retrieve Timetable data for Student");

            return Result.Failure<StudentTimetableDataDto>(PeriodErrors.NoneFoundForOffering);
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
                Day = period.DayNumber,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                TimetableName = period.Timetable,
                Name = period.Name,
                Period = period.DaySequence,
                Type = period.Type.Name
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
