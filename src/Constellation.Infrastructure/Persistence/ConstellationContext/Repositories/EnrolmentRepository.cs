using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class EnrolmentRepository : IEnrolmentRepository
    {
        private readonly AppDbContext _context;

        public EnrolmentRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Enrolment> Collection()
        {
            return _context.Enrolments
                .Include(e => e.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Room)
                .Include(e => e.Student);
        }

        public Enrolment WithDetails(int id)
        {
            return Collection()
                .FirstOrDefault(e => e.Id == id);
        }

        public Enrolment WithFilter(Expression<Func<Enrolment, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<Enrolment> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<Enrolment> AllWithFilter(Expression<Func<Enrolment, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<Enrolment> AllFromOffering(int id)
        {
            return Collection()
                .Where(e => e.OfferingId == id && e.IsDeleted == false)
                .ToList();
        }

        public ICollection<Enrolment> CurrentForStudent(string id)
        {
            return Collection()
                .Where(e => e.StudentId == id && e.IsDeleted == false)
                .ToList();
        }

        public ICollection<Enrolment> AllForStudent(string id)
        {
            return Collection()
                .Where(e => e.StudentId == id)
                .ToList();
        }

        public async Task<Enrolment> ForEditing(int id)
        {
            return await _context.Enrolments
                .Include(enrolment => enrolment.Student)
                .Include(enrolment => enrolment.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .SingleOrDefaultAsync(enrolment => enrolment.Id == id);
        }

        public async Task<bool> AnyForStudentAndOffering(string studentId, int offeringId)
        {
            return await _context.Enrolments
                .AnyAsync(enrolment => !enrolment.IsDeleted && enrolment.StudentId == studentId && enrolment.OfferingId == offeringId);
        }
    }
}