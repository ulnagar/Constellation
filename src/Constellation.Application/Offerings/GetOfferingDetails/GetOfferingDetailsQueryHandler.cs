namespace Constellation.Application.Offerings.GetOfferingDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingDetailsQueryHandler
    : IQueryHandler<GetOfferingDetailsQuery, OfferingDetailsResponse>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly ILogger _logger;

    public GetOfferingDetailsQueryHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILessonRepository lessonRepository,
        ITimetablePeriodRepository periodRepository,
        IStaffRepository staffRepository,
        IAdobeConnectRoomRepository roomRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _lessonRepository = lessonRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    public async Task<Result<OfferingDetailsResponse>> Handle(GetOfferingDetailsQuery request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.Id, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find Offering with Id {id}", request.Id);

            return Result.Failure<OfferingDetailsResponse>(OfferingErrors.NotFound(request.Id));
        }

        List<OfferingDetailsResponse.StudentSummary> students = new();

        List<Student> enrolledStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

        foreach (Student student in enrolledStudents)
        {
            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            students.Add(new(
                student.StudentId,
                student.Gender,
                student.GetName(),
                student.CurrentGrade,
                student.SchoolCode,
                school?.Name));
        }

        List<OfferingDetailsResponse.SessionSummary> sessions = new();

        foreach (Session session in offering.Sessions)
        {
            TimetablePeriod period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

            if (period is null)
            {
                _logger.Warning("Could not find Period with Id {id}", session.PeriodId);

                continue;
            }

            Staff teacher = await _staffRepository.GetById(session.StaffId, cancellationToken);

            if (teacher is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", session.StaffId);

                continue;
            }

            AdobeConnectRoom room = await _roomRepository.GetById(session.RoomId, cancellationToken);

            if (room is null)
            {
                _logger.Warning("Could not find Adobe Connect Room with Id {id}", session.RoomId);

                continue;
            }

            sessions.Add(new(
                session.Id,
                session.PeriodId,
                period.ToString(),
                $"{period.Timetable}{period.Day}{period.Period}",
                teacher.DisplayName,
                room.Name,
                room.UrlPath,
                period.Duration));
        }

        List<OfferingDetailsResponse.LessonSummary> lessons = new();

        List<SciencePracLesson> activeLessons = await _lessonRepository.GetAllForOffering(offering.Id, cancellationToken);

        List<string> studentIds = students.Select(student => student.StudentId).ToList();

        foreach (SciencePracLesson lesson in activeLessons)
        {
            List<OfferingDetailsResponse.LessonStudentAttendance> studentAttendances = new();

            List<SciencePracAttendance> attendance = lesson.Rolls
                .SelectMany(roll => roll.Attendance)
                .Where(entry => studentIds.Contains(entry.StudentId))
                .ToList();

            foreach (SciencePracAttendance entry in attendance)
            {
                OfferingDetailsResponse.StudentSummary student = students.First(student => student.StudentId == entry.StudentId);

                SciencePracRoll roll = lesson.Rolls.First(roll => roll.Id == entry.SciencePracRollId);

                studentAttendances.Add(new(
                    student.Name,
                    student.SchoolName,
                    roll.Status,
                    entry.Present,
                    roll.Comment));
            }
        }

        int fteTotal = (int)(students.Count() * offering.Course.FullTimeEquivalentValue);
        int duration = sessions.Sum(session => session.Duration);

        OfferingDetailsResponse response = new(
            offering.Id,
            OfferingName.FromValue(offering.Name),
            offering.CourseId,
            offering.Course.Name,
            offering.Course.Grade,
            offering.StartDate,
            offering.EndDate,
            offering.IsCurrent,
            students,
            sessions,
            lessons,
            fteTotal,
            duration);

        return response;
    }
}
