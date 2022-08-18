using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class ClassworkNotificationRepository : IClassworkNotificationRepository
    {
        private readonly AppDbContext _context;

        public ClassworkNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClassworkNotification> Get(Guid id)
        {
            return await _context.ClassworkNotifications
                .Include(notification => notification.Absences)
                .ThenInclude(absence => absence.Student)
                .Include(notification => notification.Offering)
                .Include(notification => notification.Offering.Course)
                .Include(notification => notification.Covers)
                .Include(notification => notification.Teachers)
                .SingleOrDefaultAsync(notification => notification.Id == id);
        }

        public async Task<ICollection<ClassworkNotification>> GetAll()
        {
            return await _context.ClassworkNotifications
                .Include(notification => notification.Absences)
                .Include(notification => notification.Offering)
                .Include(notification => notification.Covers)
                .Include(notification => notification.Teachers)
                .ToListAsync();
        }

        public async Task<ClassworkNotification> GetForDuplicateCheck(int offeringId, DateTime absenceDate)
        {
            return await _context.ClassworkNotifications
                .Include(notification => notification.Absences)
                .Include(notification => notification.Offering)
                .Include(notification => notification.Covers)
                .Include(notification => notification.Teachers)
                .SingleOrDefaultAsync(notification => notification.AbsenceDate == absenceDate && notification.OfferingId == offeringId);
        }

        public async Task<ICollection<ClassworkNotification>> GetOutstandingForTeacher(string staffId)
        {
            return await _context.ClassworkNotifications
                .Include(notification => notification.Absences)
                .ThenInclude(absence => absence.Student)
                .Include(notification => notification.Offering)
                .Include(notification => notification.Offering.Course)
                .Where(notification => notification.Teachers.Any(teacher => teacher.StaffId == staffId) && !notification.CompletedAt.HasValue)
                .ToListAsync();
        }

        public async Task<ICollection<ClassworkNotification>> GetForTeacher(string staffId)
        {
            return await _context.ClassworkNotifications
                .Include(notification => notification.Offering)
                .Include(notification => notification.Absences)
                .ThenInclude(absence => absence.Student)
                .Where(notification => notification.Teachers.Any(staff => staff.StaffId == staffId))
                .ToListAsync();
        }
    }
}
