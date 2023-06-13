﻿namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.MissedWork;
using Constellation.Core.Shared;

public class AbsenceClassworkNotificationJob : IAbsenceClassworkNotificationJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IClassworkNotificationRepository _notificationRepository;
    private readonly ILogger _logger;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IStaffRepository _staffRepository;

    public AbsenceClassworkNotificationJob(
        IUnitOfWork unitOfWork,
        IAbsenceRepository absenceRepository,
        ICourseOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        IClassworkNotificationRepository notificationRepository,
        IClassCoverRepository classCoverRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _notificationRepository = notificationRepository;
        _logger = logger.ForContext<IAbsenceClassworkNotificationJob>();
        _classCoverRepository = classCoverRepository;
        _staffRepository = staffRepository;
    }

    public async Task StartJob(Guid jobId, DateOnly scanDate, CancellationToken token)
    {
        _logger.Information("{id}: Starting missed classwork notifications scan", jobId);

        List<Absence> absencesFromDb = await _absenceRepository.GetWholeAbsencesForScanDate(scanDate);
        List<Absence> absences = new();

        List<CourseOffering> offerings = await _offeringRepository.GetAllActive(token);
        List<Course> courses = await _courseRepository.GetAll(token);

        foreach (Absence absence in absencesFromDb)
        {
            CourseOffering offering = offerings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

            if (offering is null)
                continue;

            Course course = courses.FirstOrDefault(course => course.Id == offering.CourseId);

            if (course is null)
                continue;

            if (course.Name == "Tutorial" ||
                course.Grade == Grade.Y05 ||
                course.Grade == Grade.Y06)
                continue;

            absences.Add(absence);
        }

        var absencesByClassAndDate = absences.GroupBy(absence => new { absence.OfferingId, absence.Date });

        foreach (var group in absencesByClassAndDate)
        {
            if (token.IsCancellationRequested)
                return;

            DateOnly absenceDate = group.Key.Date;
            int offeringId = group.Key.OfferingId;

            // Make sure to filter out cancelled sessions and deleted teachers
            List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offeringId, token);
            List<string> studentIds = group
                .Select(absence => absence.StudentId)
                .Distinct()
                .ToList();

            List<Student> students = await _studentRepository.GetListFromIds(studentIds, token);
            var offering = offerings.FirstOrDefault(offering => offering.Id == offeringId);

            var covers = await _classCoverRepository.GetAllForDateAndOfferingId(absenceDate, offeringId, token);

            if (covers.Count > 0)
            {
                var course = courses.FirstOrDefault(course => course.Id == offering.Id);

                teachers = await _staffRepository.GetFacultyHeadTeachers(course.FacultyId, token);
            }

            List<ClassworkNotification> existing = await _notificationRepository.GetForOfferingAndDate(offeringId, absenceDate, token);

            if (!existing.Any())
            {
                // create notification object in database
                var notificationRequest = ClassworkNotification.Create(
                    group.ToList(),
                    teachers,
                    covers.Any());

                if (notificationRequest.IsFailure)
                {
                    _logger.Warning("Could not create Classwork Notification: {error} - {message}", notificationRequest.Error.Code, notificationRequest.Error.Message);
                    continue;
                }

                _notificationRepository.Insert(notificationRequest.Value);

                await _unitOfWork.CompleteAsync(token);

                continue;
            }

            // Somehow, and this should not happen, there is already an entry for this occurance?
            // Compare the list of students to see who has been added (or removed) and update the database
            // entry accordingly. Then, if the teacher has already responded, send the student/parent email
            // with the details of work required.

            _logger.Information("{id}: Found existing entries for {Offering} @ {AbsenceDate}", jobId, offering.Name, absenceDate.ToShortDateString());

            List<Absence> newAbsences = new();

            // Playground
            ClassworkNotification check = existing.OrderBy(entry => entry.GeneratedAt).First();

            if (check.CompletedAt.HasValue)
            {
                // create notification object in database
                Result<ClassworkNotification> notificationRequest = ClassworkNotification.Create(
                    group.ToList(),
                    teachers,
                    covers.Any());

                if (notificationRequest.IsFailure)
                {
                    _logger.Warning("Could not create Classwork Notification: {error} - {message}", notificationRequest.Error.Code, notificationRequest.Error.Message);
                    continue;
                }

                _notificationRepository.Insert(notificationRequest.Value);

                await _unitOfWork.CompleteAsync(token);

                continue;
            }
                
            // Update existing
            foreach (Absence absence in check.Absences)
            {
                if (!check.Absences.Contains(absence))
                {
                    newAbsences.Add(absence);

                    _logger.Information("{id}: Adding {Student} to the existing entry for {AbsenceDate}", jobId, absence.StudentId, check.AbsenceDate.ToShortDateString());
                }
            }

            foreach (Absence absence in newAbsences)
                check.AddAbsence(absence);

            await _unitOfWork.CompleteAsync(token);
        }
    }
}
