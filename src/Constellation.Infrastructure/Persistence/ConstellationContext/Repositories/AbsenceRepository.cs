using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class AbsenceRepository : IAbsenceRepository
    {
        private readonly AppDbContext _context;

        public AbsenceRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Absence> Collection()
        {
            return _context.Absences
                .Include(a => a.Student)
                .ThenInclude(student => student.School)
                .Include(a => a.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .Include(a => a.Notifications)
                .Include(a => a.Responses);
        }

        public async Task<ICollection<Absence>> All()
        {
            return await Collection().ToListAsync();
        }

        public async Task<ICollection<Absence>> AllWithFilter(Expression<Func<Absence, bool>> predicate)
        {
            return await Collection()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Absence> WithDetails(string id)
        {
            return await Collection().SingleOrDefaultAsync(s => s.Id.ToString() == id);
        }

        public Absence WholeWithDetails(string id)
        {
            return Collection()
                .SingleOrDefault(s => s.Id.ToString() == id);
        }

        public Absence WholeWithFilter(Expression<Func<Absence, bool>> predicate)
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Whole)
                .FirstOrDefault(predicate);
        }

        public ICollection<Absence> AllWholeWithFilter(Expression<Func<Absence, bool>> predicate)
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Whole)
                .Where(predicate)
                .ToList();
        }

        public ICollection<Absence> AllWhole()
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Whole)
                .ToList();
        }

        public Absence PartialWithDetails(string id)
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Partial)
                .SingleOrDefault(s => s.Id.ToString() == id);
        }

        public Absence PartialWithFilter(Expression<Func<Absence, bool>> predicate)
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Partial)
                .FirstOrDefault(predicate);
        }

        public ICollection<Absence> AllPartialWithFilter(Expression<Func<Absence, bool>> predicate)
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Partial)
                .Where(predicate)
                .ToList();
        }

        public ICollection<Absence> AllPartial()
        {
            return Collection()
                .Where(absence => absence.Type == Absence.Partial)
                .ToList();
        }

        public async Task<ICollection<Absence>> AllFromSchoolForCoordinatorPortal(string schoolCode)
        {
            return await _context.Absences
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Responses)
                .Include(absence => absence.Offering)
                .Where(absence => absence.Student.SchoolCode == schoolCode)
                .ToListAsync();
        }

        public async Task<ICollection<Absence>> AllFromStudentForParentPortal(string studentId)
        {
            return await _context.Absences
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Notifications)
                .Include(absence => absence.Responses)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Course)
                .Where(absence => absence.StudentId == studentId && absence.Date.Year == DateTime.Now.Year)
                .ToListAsync();
        }

        public async Task<Absence> ForExplanationFromParent(Guid id)
        {
            return await _context.Absences
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Notifications)
                .Include(absence => absence.Responses)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Course)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleOrDefaultAsync(absence => absence.Id == id);
        }

        public async Task<Absence> ForExplanationFromStudent(Guid id)
        {
            return await _context.Absences
                .Include(absence => absence.Responses)
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .ThenInclude(school => school.StaffAssignments)
                .ThenInclude(assignment => assignment.SchoolContact)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleOrDefaultAsync(absence => absence.Id == id);
        }

        public async Task<AbsenceResponse> AsResponseForVerificationByCoordinator(Guid id)
        {
            //ID is AbsenceResponse.Id, not Absence.Id!

            var absence = await _context.Absences
                .Include(absence => absence.Responses)
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .ThenInclude(school => school.StaffAssignments)
                .ThenInclude(assignment => assignment.SchoolContact)
                .Include(absence => absence.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleOrDefaultAsync(absence => absence.Responses.Any(response => response.Id == id));

            return absence.Responses.First(response => response.Id == id);
        }


        public async Task<Absence> ForSendingNotificationAsync(string id)
        {
            return await _context.Absences
                .Include(absence => absence.Notifications)
                .Include(absence => absence.Responses)
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Offering)
                .SingleOrDefaultAsync(absence => absence.Id == new Guid(id));
        }

        public async Task<ICollection<Absence>> ForReportAsync(AbsenceFilterDto filter)
        {
            return await _context.Absences
                .Include(absence => absence.Notifications)
                .Include(absence => absence.Responses)
                .Include(absence => absence.Student)
                .ThenInclude(student => student.School)
                .Include(absence => absence.Offering)
                .Where(FilterAbsence(filter))
                .ToListAsync();
        }

        public async Task<ICollection<Absence>> ForClassworkNotifications(DateTime scanDate)
        {
            return await _context.Absences
                .Include(absence => absence.Student)
                .Include(absence => absence.Student.School)
                .Include(absence => absence.Offering)
                .Include(absence => absence.Offering.Course)
                .Include(absence => absence.Offering.Course.HeadTeacher)
                .Include(absence => absence.Offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .Where(absence =>
                    absence.DateScanned == scanDate &&
                    absence.Type == Absence.Whole &&
                    absence.Offering.Course.Name != "Tutorial" &&
                    absence.Offering.Course.Grade != Grade.Y05 &&
                    absence.Offering.Course.Grade != Grade.Y06)
                .ToListAsync();
        }

        public async Task<ICollection<Absence>> ForStudentWithinTimePeriod(string studentId, DateTime startDate, DateTime endDate)
        {
            return await _context.Absences
                .Include(absence => absence.Responses)
                .Where(absence => absence.StudentId == studentId && absence.Date >= startDate && absence.Date <= endDate)
                .ToListAsync();
        }

        private static Expression<Func<Absence, bool>> FilterAbsence(AbsenceFilterDto filter)
        {
            var predicate = PredicateBuilder.New<Absence>();

            if (!string.IsNullOrWhiteSpace(filter.StudentId))
            {
                predicate.And(absence => absence.StudentId == filter.StudentId);
            }

            if (!string.IsNullOrWhiteSpace(filter.SchoolCode))
            {
                predicate.And(absence => absence.Student.SchoolCode == filter.SchoolCode);
            }

            if (filter.Grade.HasValue)
            {
                predicate.And(absence => absence.Student.CurrentGrade == (Grade)filter.Grade);
            }

            if (filter.StartDate.HasValue)
            {
                predicate.And(absence => absence.Date.Date >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                predicate.And(absence => absence.Date.Date <= filter.EndDate.Value.Date);
            }

            return predicate;
        }
    }
}
