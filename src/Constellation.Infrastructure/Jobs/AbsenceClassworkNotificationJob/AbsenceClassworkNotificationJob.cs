#nullable enable
namespace Constellation.Infrastructure.Jobs.AbsenceClassworkNotificationJob;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Models.EmailQueue;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;

public class AbsenceClassworkNotificationJob : IAbsenceClassworkNotificationJob, IScopedService
{
    private readonly ILogger _logger;
    private readonly IAbsenceClassworkNotificationJobDataHandler _handler;

    public AbsenceClassworkNotificationJob(IAbsenceClassworkNotificationJobDataHandler handler, ILogger logger)
    {
        _logger = logger.ForContext<IAbsenceMonitorJob>();
        _handler = handler;
    }

    public async Task StartJob(Guid jobId, DateTime scanDate, CancellationToken token)
    {
        _logger.Information("{id}: Starting missed classwork notifications scan", jobId);

        var absences = await _handler.GetAbsences(scanDate, token);

        var absencesByClassAndDate = absences.GroupBy(absence => new { absence.OfferingId, absence.Date.Date });

        foreach (var group in absencesByClassAndDate)
        {
            if (token.IsCancellationRequested)
                return;

            var absenceDate = group.Key.Date;
            var offeringId = group.Key.OfferingId;

            // Check if the db already has an entry for this?
            var check = await _handler.GetClassworkNotification(absenceDate, offeringId, token);

            if (check == null)
            {
                // Make sure to filter out cancelled sessions and deleted teachers
                var teachers = group.First().Offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher).Where(teacher => !teacher.IsDeleted).Distinct().ToList();
                var offering = group.First().Offering;

                List<ClassCover> covers = await _handler.GetCovers(absenceDate, offeringId, token);

                // If there are covers, send the email to the Head Teacher instead
                if (covers.Count > 0)
                    teachers = new List<Staff> { offering.Course.HeadTeacher };

                // create notification object in database
                var notification = new ClassworkNotification
                {
                    AbsenceDate = absenceDate,
                    Absences = group.ToList(),
                    Covers = covers,
                    Offering = offering,
                    OfferingId = offeringId,
                    Teachers = teachers
                };

                await ProcessNewNotification(jobId, notification, token);
            }
            else
            {
                // Somehow, and this should not happen, there is already an entry for this occurance?
                // Compare the list of students to see who has been added (or removed) and update the database
                // entry accordingly. Then, if the teacher has already responded, send the student/parent email
                // with the details of work required.

                _logger.Information("{id}: Found existing entry for {Offering} @ {AbsenceDate}", jobId, check.Offering.Name, check.AbsenceDate.ToShortDateString());

                foreach (var absence in group)
                    await CheckAbsenceInExistingNotification(jobId, absence, check, token);
            }
        }
    }

    private async Task CheckAbsenceInExistingNotification(Guid jobId, Models.AbsenceDto absence, ClassworkNotification dbNotification, CancellationToken token)
    {
        if (!dbNotification.Absences.Any(dbAbsence => dbAbsence.Id == absence.Id))
        {
            _logger.Information("{id}: Adding {Student} to the existing entry for {Offering} @ {AbsenceDate}", jobId, absence.Student.DisplayName, dbNotification.Offering.Name, dbNotification.AbsenceDate.ToShortDateString());

            // Add absence to the dbNotification (?)
            await _handler.SaveAbsenceToNotification(dbNotification.Id, absence, token);

            if (dbNotification.CompletedBy != null)
            {
                // Send email to students and parents
                var parentEmails = await _handler.GetParentEmailAddresses(absence, token);

                var emailNotification = new ClassworkNotificationStudentEmail
                {
                    Absence = absence,
                    Notification = dbNotification,
                    ParentEmails = parentEmails
                };

                var entry = new EmailQueueItem();
                entry.StoreData(emailNotification);
                entry.ScheduledFor = DateTime.Now;

                await _handler.SaveEmail(entry, token);
            }
        }
    }

    private async Task ProcessNewNotification(Guid jobId, ClassworkNotification notification, CancellationToken token)
    {
        await _handler.SaveClassworkNotification(notification, token);

        foreach (var teacher in notification.Teachers)
            _logger.Information("{id}: Sending email for {Offering} @ {AbsenceDate} to {teacher} ({EmailAddress})", jobId, notification.Offering.Name, notification.AbsenceDate.ToShortDateString(), teacher.DisplayName, teacher.EmailAddress);

        var emailNotification = new ClassworkNotificationTeacherEmail
        {
            NotificationId = notification.Id,
            OfferingName = notification.Offering.Name,
            Students = notification.Absences.Select(absence => absence.Student.DisplayName).ToList(),
            AbsenceDate = notification.AbsenceDate,
            Teachers = notification.Teachers.Select(teacher => new ClassworkNotificationTeacherEmail.Teacher { Name = teacher.DisplayName, Email = teacher.EmailAddress }).ToList(),
            IsCovered = notification.Covers.Any()
        };

        var entry = new EmailQueueItem();
        entry.StoreData(emailNotification);
        entry.ScheduledFor = DateTime.Now;

        await _handler.SaveEmail(entry, token);
    }
}
