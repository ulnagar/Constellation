namespace Constellation.Application.Offerings.GetOfferingDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Enums;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingDetailsQueryHandler
    : IQueryHandler<GetOfferingDetailsQuery, OfferingDetailsResponse>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetOfferingDetailsQueryHandler(
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILessonRepository lessonRepository,
        IPeriodRepository periodRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _lessonRepository = lessonRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
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

        Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

        if (course is null)
        {
            _logger.Warning("Could not find Course with Id {id}", offering.CourseId);

            return Result.Failure<OfferingDetailsResponse>(CourseErrors.NotFound(offering.CourseId));
        }

        List<OfferingDetailsResponse.StudentSummary> students = new();

        List<Student> enrolledStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

        foreach (Student student in enrolledStudents)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            bool currentEnrolment = true;

            if (enrolment is null)
            {
                // retrieve most recent applicable school enrolment
                if (student.SchoolEnrolments.Count > 0)
                {
                    currentEnrolment = false;

                    int maxYear = student.SchoolEnrolments.Max(item => item.Year);

                    SchoolEnrolmentId enrolmentId = student.SchoolEnrolments
                        .Where(entry => entry.Year == maxYear)
                        .Select(entry => new { entry.Id, Date = entry.EndDate ?? DateOnly.MaxValue })
                        .MaxBy(entry => entry.Date)
                        .Id;

                    enrolment = student.SchoolEnrolments.FirstOrDefault(entry => entry.Id == enrolmentId);
                }
            }

            students.Add(new(
                student.Id,
                student.StudentReferenceNumber,
                student.Gender,
                student.Name,
                enrolment?.Grade,
                enrolment?.SchoolCode,
                enrolment?.SchoolName,
                currentEnrolment));
        }

        List<OfferingDetailsResponse.SessionSummary> sessions = new();

        foreach (Session session in offering.Sessions)
        {
            if (session.IsDeleted)
                continue;

            Period period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

            if (period is null)
            {
                _logger.Warning("Could not find Period with Id {id}", session.PeriodId);

                continue;
            }

            sessions.Add(new(
                session.Id,
                session.PeriodId,
                period.ToString(),
                period.SortOrder,
                period.Duration));
        }

        List<OfferingDetailsResponse.TeacherSummary> teachers = new();

        foreach (TeacherAssignment assignment in offering.Teachers)
        {
            if (assignment.IsDeleted)
                continue;

            Staff teacher = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

            if (teacher is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", assignment.StaffId);

                continue;
            }

            teachers.Add(new(
                teacher.StaffId,
                teacher.GetName(),
                assignment.Type));
        }

        List<OfferingDetailsResponse.ResourceSummary> resources = new();

        foreach (Resource resource in offering.Resources)
        {
            resources.Add(new(
                resource.Id,
                resource.Type,
                resource.Name,
                resource.Url));
        }

        List<OfferingDetailsResponse.LessonSummary> lessons = new();

        List<SciencePracLesson> activeLessons = await _lessonRepository.GetAllForOffering(offering.Id, cancellationToken);

        List<StudentId> studentIds = students.Select(student => student.StudentId).ToList();

        foreach (SciencePracLesson lesson in activeLessons)
        {
            if (lesson.Rolls.All(roll => roll.Status == LessonStatus.Cancelled))
                continue;

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

            lessons.Add(new(
                lesson.Id,
                lesson.DueDate,
                lesson.Name,
                studentAttendances));
        }

        decimal fteTotal = (students.Count() * course.FullTimeEquivalentValue);
        int duration = sessions.Sum(session => session.Duration);
        
        Result<OfferingName> offeringName = OfferingName.FromValue(offering.Name);

        if (offeringName.IsFailure)
        {
            _logger
                .ForContext(nameof(GetOfferingDetailsQuery), request, true)
                .ForContext(nameof(Error), offeringName.Error, true)
                .Warning("Failed to retrieve Offering");

            return Result.Failure<OfferingDetailsResponse>(offeringName.Error);
        }

        OfferingDetailsResponse response = new(
            offering.Id,
            offeringName.Value,
            offering.CourseId,
            course.Name,
            course.Grade,
            offering.StartDate,
            offering.EndDate,
            offering.IsCurrent,
            students,
            sessions,
            lessons,
            teachers,
            resources,
            fteTotal,
            duration);

        return response;
    }
}
