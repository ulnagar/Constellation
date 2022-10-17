#nullable enable
namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.EmailQueue;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class AbsenceClassworkNotificationJob : IAbsenceClassworkNotificationJob, IScopedService
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AbsenceClassworkNotificationJob(IUnitOfWork unitOfWork, ILogger logger)
    {
        _logger = logger.ForContext<IAbsenceMonitorJob>();
        _unitOfWork = unitOfWork;
    }

    public async Task StartJob(Guid jobId, DateTime scanDate, CancellationToken token)
    {
        _logger.Information("{id}: Starting missed classwork notifications scan", jobId);

        var absences = await GetAbsences(scanDate, token);

        var absencesByClassAndDate = absences.GroupBy(absence => new { absence.OfferingId, absence.Date.Date });

        foreach (var group in absencesByClassAndDate)
        {
            if (token.IsCancellationRequested)
                return;

            var absenceDate = group.Key.Date;
            var offeringId = group.Key.OfferingId;

            // Make sure to filter out cancelled sessions and deleted teachers
            var teachers = group.First().Offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher).Where(teacher => !teacher.IsDeleted).Distinct().ToList();
            var offering = group.First().Offering;

            List<ClassCover> covers = await GetCovers(absenceDate, offeringId, token);

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

            if (token.IsCancellationRequested)
                return;

            // Check if the db already has an entry for this?
            var check = await GetClassworkNotification(absenceDate, offeringId, token);

            if (check == null)
            {
                await ProcessNewNotification(jobId, notification, token);
            }
            else
            {
                // Somehow, and this should not happen, there is already an entry for this occurance?
                // Compare the list of students to see who has been added (or removed) and update the database
                // entry accordingly. Then, if the teacher has already responded, send the student/parent email
                // with the details of work required.

                await ProcessExistingNotification(jobId, notification, check, token);
            }
        }
    }

    private async Task ProcessExistingNotification(Guid jobId, ClassworkNotification notification, ClassworkNotification check, CancellationToken token)
    {
        _logger.Information("{id}: Found existing entry for {Offering} @ {AbsenceDate}", jobId, notification.Offering.Name, notification.AbsenceDate.ToShortDateString());

        var newAbsences = new List<Absence>();

        foreach (var absence in notification.Absences)
        {
            if (!check.Absences.Contains(absence))
            {
                newAbsences.Add(absence);

                _logger.Information("{id}: Adding {Student} to the existing entry for {Offering} @ {AbsenceDate}", jobId, absence.Student.DisplayName, notification.Offering.Name, notification.AbsenceDate.ToShortDateString());
            }
        }

        if (check.CompletedBy != null)
        {
            // Teacher has already submitted response. Add new students and send their email directly
            foreach (var absence in newAbsences)
            {
                check.Absences.Add(absence);
                var parentEmails = await GetParentEmailAddresses(absence, token);

                var emailNotification = new ClassworkNotificationStudentEmail
                {
                    Absence = absence,
                    Notification = check,
                    ParentEmails = parentEmails
                };

                var entry = new EmailQueueItem();
                entry.StoreData(emailNotification);
                entry.ScheduledFor = DateTime.Now;

                await SaveEmail(entry, token);
            }
        }
        else
        {
            foreach (var absence in newAbsences)
                check.Absences.Add(absence);
        }

        await _context.SaveChangesAsync(token);
    }

    internal async Task ProcessNewNotification(Guid jobId, ClassworkNotification notification, CancellationToken token)
    {
        await SaveClassworkNotification(notification, token);

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

        await SaveEmail(entry, token);
    }

    internal async Task<ClassworkNotification?> GetClassworkNotification(DateTime absenceDate, int offeringId, CancellationToken token) =>
        await _context.ClassworkNotifications
                .Include(notification => notification.Absences)
                .Include(notification => notification.Offering)
                .Include(notification => notification.Covers)
                .Include(notification => notification.Teachers)
                .SingleOrDefaultAsync(notification => notification.AbsenceDate == absenceDate && notification.OfferingId == offeringId, token);

    internal async Task<List<ClassCover>> GetCovers(DateTime absenceDate, int offeringId, CancellationToken token) =>
        await _context.Covers
                .Include(cover => ((CasualClassCover)cover).Casual)
                .Include(cover => ((TeacherClassCover)cover).Staff)
                .Where(cover => !cover.IsDeleted &&
                    cover.StartDate <= absenceDate &&
                    cover.EndDate >= absenceDate &&
                    cover.OfferingId == offeringId)
                .ToListAsync(token);

    internal async Task<List<string>> GetParentEmailAddresses(Absence absence, CancellationToken token)
    {
        var families = await GetFamilies(absence.StudentId, token);

        List<string> parentEmails = new();

        foreach (var family in families)
        {
            if (!string.IsNullOrWhiteSpace(family.Parent1.EmailAddress))
                parentEmails.Add(family.Parent1.EmailAddress);

            if (!string.IsNullOrWhiteSpace(family.Parent2.EmailAddress))
                parentEmails.Add(family.Parent2.EmailAddress);
        }

        return parentEmails.Distinct().ToList();
    }

    internal async Task<List<StudentFamily>> GetFamilies(string studentId, CancellationToken token) =>
        await _context.StudentFamilies
            .Where(family => family.Students.Any(student => student.StudentId == studentId))
            .ToListAsync(token);

    internal async Task<ICollection<Absence>> GetAbsences(DateTime scanDate, CancellationToken token) =>
        await _context.Absences
                .Include(absence => absence.Student)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Course)
                .ThenInclude(course => course.HeadTeacher)
                .Include(absence => absence.Offering.Sessions.Where(session => !session.IsDeleted))
                .ThenInclude(session => session.Teacher)
                .Where(absence =>
                    absence.DateScanned == scanDate &&
                    absence.Type == Absence.Whole &&
                    absence.Offering.Course.Name != "Tutorial" &&
                    absence.Offering.Course.Grade != Grade.Y05 &&
                    absence.Offering.Course.Grade != Grade.Y06)
                .ToListAsync(token);

    internal async Task SaveClassworkNotification(ClassworkNotification notification, CancellationToken token)
    {
        _context.ClassworkNotifications.Add(notification);
        await _context.SaveChangesAsync(token);
    }

    internal async Task SaveEmail(EmailQueueItem item, CancellationToken token)
    {
        _context.EmailQueue.Add(item);
        await _context.SaveChangesAsync(token);
    }
}
