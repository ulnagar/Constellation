#nullable enable
namespace Constellation.Infrastructure.Jobs.AbsenceClassworkNotificationJob;

using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.EmailQueue;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;

public class AbsenceClassworkNotificationJobDataHandler : IAbsenceClassworkNotificationJobDataHandler
{
    private readonly IAppDbContext _context;

    public AbsenceClassworkNotificationJobDataHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ClassworkNotification?> GetClassworkNotification(DateTime absenceDate, int offeringId, CancellationToken token) =>
        await _context.ClassworkNotifications
            .Include(notification => notification.Absences)
            .Include(notification => notification.Offering)
            .Include(notification => notification.Covers)
            .Include(notification => notification.Teachers)
            .SingleOrDefaultAsync(notification => notification.AbsenceDate == absenceDate && notification.OfferingId == offeringId, token);

    public async Task<List<ClassCover>> GetCovers(DateTime absenceDate, int offeringId, CancellationToken token) =>
        await _context.Covers
                .Include(cover => ((CasualClassCover)cover).Casual)
                .Include(cover => ((TeacherClassCover)cover).Staff)
                .Where(cover => !cover.IsDeleted &&
                    cover.StartDate <= absenceDate &&
                    cover.EndDate >= absenceDate &&
                    cover.OfferingId == offeringId)
                .ToListAsync(token);

    public async Task<List<string>> GetParentEmailAddresses(Absence absence, CancellationToken token)
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

    public async Task<List<StudentFamily>> GetFamilies(string studentId, CancellationToken token) =>
        await _context.StudentFamilies
            .Where(family => family.Students.Any(student => student.StudentId == studentId))
            .ToListAsync(token);

    public async Task<ICollection<Absence>> GetAbsences(DateTime scanDate, CancellationToken token) =>
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

    public async Task SaveClassworkNotification(ClassworkNotification notification, CancellationToken token)
    {
        _context.ClassworkNotifications.Add(notification);
        await _context.SaveChangesAsync(token);
    }

    public async Task SaveEmail(EmailQueueItem item, CancellationToken token)
    {
        _context.EmailQueue.Add(item);
        await _context.SaveChangesAsync(token);
    }

    public async Task SaveAbsenceToNotification(Guid notificationId, Absence absence, CancellationToken token)
    {
        var entity = new ClassworkNotification
        {
            Id = notificationId
        };

        entity.Absences.Add(absence);

        await _context.SaveChangesAsync(token);
    }
}
