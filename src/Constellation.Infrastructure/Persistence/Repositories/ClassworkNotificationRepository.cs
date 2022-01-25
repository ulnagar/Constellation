using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
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
                .Include(notification => notification.Absences.Select(absence => absence.Student))
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
    }
}
