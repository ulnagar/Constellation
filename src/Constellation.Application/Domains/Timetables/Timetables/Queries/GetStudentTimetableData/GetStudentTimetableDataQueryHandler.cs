namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;

using Abstractions.Messaging;
using Core.Extensions;
using Core.Models;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
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
using Serilog;
using System;
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
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public GetStudentTimetableDataQueryHandler(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IStaffRepository staffRepository,
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
        _planRepository = planRepository;
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

        AttendancePlan? plan = await _planRepository.GetCurrentApprovedForStudent(student.Id, cancellationToken);

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
        response.HasAttendancePlan = plan is not null;

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

            AttendancePlanPeriod? planPeriod = plan?.Periods.FirstOrDefault(planPeriod => planPeriod.PeriodId == period.Id);

            TimetableDataDto.TimetableData entry = new()
            {
                Timetable = period.Timetable,
                Week = period.Week,
                Day = period.Day,
                StartTime = period.StartTime,
                EntryTime = planPeriod?.EntryTime ?? TimeOnly.MinValue,
                EndTime = period.EndTime,
                ExitTime = planPeriod?.ExitTime ?? TimeOnly.MinValue,
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

        return response;
    }
}
