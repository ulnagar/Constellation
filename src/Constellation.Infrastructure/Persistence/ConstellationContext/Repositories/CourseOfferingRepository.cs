using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class CourseOfferingRepository : ICourseOfferingRepository
    {
        private readonly AppDbContext _context;

        public CourseOfferingRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<CourseOffering> Collection()
        {
            return _context.Offerings
                .Include(o => o.Course)
                    .ThenInclude(course => course.Faculty)
                        .ThenInclude(faculty => faculty.Members.Where(member => member.Role == FacultyMembershipRole.Manager))
                            .ThenInclude(member => member.Staff)
                .Include(o => o.Sessions)
                    .ThenInclude(session => session.Room)
                .Include(o => o.Sessions)
                    .ThenInclude(session => session.Period)
                .Include(o => o.Sessions)
                    .ThenInclude(session => session.Teacher)
                .Include(o => o.Enrolments)
                    .ThenInclude(enrolment => enrolment.Student)
                        .ThenInclude(student => student.AdobeConnectOperations)
                .Include(o => o.Enrolments)
                    .ThenInclude(enrolment => enrolment.Student)
                        .ThenInclude(student => student.School)
                .Include(o => o.Resources);
        }

        public async Task<CourseOffering?> GetById(
            int offeringId, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<CourseOffering>()
                .Where(offering => offering.Id == offeringId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<CourseOffering>> GetAllActive(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<CourseOffering>()
                .Where(offering => 
                    offering.StartDate <= DateTime.Now && 
                    offering.EndDate >= DateTime.Now)
                .ToListAsync(cancellationToken);

        public CourseOffering WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public CourseOffering WithFilter(Expression<Func<CourseOffering, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public async Task<CourseOffering> GetForExistCheck(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions) // Required for CoverService and EnrolmentService to create operations
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public ICollection<CourseOffering> All()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .ToList();
        }

        public ICollection<CourseOffering> AllWithFilter(Expression<Func<CourseOffering, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<CourseOffering> AllCurrentOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now)
                .ToList();
        }

        public ICollection<CourseOffering> AllFutureOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.StartDate > DateTime.Now)
                .ToList();
        }

        public ICollection<CourseOffering> AllPastOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate < DateTime.Now)
                .ToList();
        }

        public ICollection<CourseOffering> AllFromFaculty(Faculty faculty)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Course.Faculty == faculty)
                .ToList();
        }

        public ICollection<CourseOffering> AllFromGrade(Grade grade)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Course.Grade == grade)
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now || o.StartDate > DateTime.Now)
                .ToList();
        }

        public ICollection<CourseOffering> AllForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<CourseOffering> AllCurrentAndFutureForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate > DateTime.Today)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<CourseOffering> AllCurrentForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate > DateTime.Today && o.StartDate < DateTime.Today)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<CourseOffering> AllForTeacher(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Sessions.Any(s => s.StaffId == id && !s.IsDeleted))
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now)
                .ToList();
        }

        public async Task<ICollection<CourseOffering>> AllForTeacherAsync(string id)
        {
            return await _context.Offerings
                .Where(offering => offering.Sessions.Any(s => s.StaffId == id && !s.IsDeleted) && offering.StartDate < DateTime.Now && offering.EndDate > DateTime.Now)
                .OrderBy(offering => offering.Name)
                .ThenBy(offering => offering.StartDate)
                .ToListAsync();
        }

        public ICollection<Staff> AllClassTeachers(int id)
        {
            var sessions = _context.Sessions
                .Include(s => s.Teacher)
                .Where(s => s.OfferingId == id && !s.IsDeleted);

            var teachers = sessions
                .Select(c => c.Teacher)
                .Distinct()
                .ToList();

            return teachers;
        }

        public async Task<ICollection<CourseOffering>> ForSelectionAsync()
        {
            return await _context.Offerings
                .Where(offering => offering.EndDate > DateTime.Now && offering.StartDate < DateTime.Now)
                .ToListAsync();
        }

        public async Task<ICollection<CourseOffering>> FromGradeForBulkEnrolAsync(Grade grade)
        {
            return await _context.Offerings
                .Include(offering => offering.Course)
                .Where(offering => offering.Course.Grade == grade && offering.EndDate >= DateTime.Now)
                .ToListAsync();
        }

        public async Task<CourseOffering> ForEnrolmentAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<ICollection<CourseOffering>> ForListAsync(Expression<Func<CourseOffering, bool>> predicate)
        {
            return await _context.Offerings
                .Include(offering => offering.Course)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Period)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<CourseOffering> ForDetailDisplayAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Course)
                .Include(offering => offering.Enrolments)
                .ThenInclude(enrolment => enrolment.Student)
                .ThenInclude(student => student.School)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Period)
                .Include(offering => offering.Lessons)
                .ThenInclude(lesson => lesson.Rolls)
                .ThenInclude(roll => roll.Attendance)
                .ThenInclude(attend => attend.Student)
                .ThenInclude(student => student.School)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<CourseOffering> ForEditAsync(int id)
        {
            return await _context.Offerings
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<CourseOffering> ForSessionEditAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<CourseOffering> ForRollCreationAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Enrolments)
                .ThenInclude(enrolment => enrolment.Student)
                .ThenInclude(student => student.School)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<CourseOffering> ForCoverCreationAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Period)
                .Include(offering => offering.Course)
                .ThenInclude(course => course.Faculty)
                .ThenInclude(faculty => faculty.Members.Where(member => member.Role == FacultyMembershipRole.Manager))
                .ThenInclude(member => member.Staff)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<ICollection<Staff>> AllTeachersForCoverCreationAsync(int id)
        {
            var offering = await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleAsync(offering => offering.Id == id);

            var teachers = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher);

            return teachers.ToList();
        }

        public async Task<CourseOffering> ForSessionCreationAsync(int id)
        {
            return await _context.Offerings
                .Include(offering => offering.Enrolments)
                .ThenInclude(enrolment => enrolment.Student)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<bool> AnyWithId(int id)
        {
            return await _context.Offerings
                .AnyAsync(offering => offering.Id == id);
        }

        public async Task<ICollection<CourseOffering>> ForTeacherAndDates(string staffId, ICollection<int> dates)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Period)
                .Where(offering => offering.Sessions.Any(session => session.StaffId == staffId && !session.IsDeleted && dates.Contains(session.Period.Day))
                    && offering.StartDate < DateTime.Now
                    && offering.EndDate > DateTime.Now)
                .ToListAsync();
        }

        public async Task<CourseOffering> GetFromYearAndName(int year, string name)
        {
            return await _context.Offerings
                .Include(offering => offering.Course)
                    .ThenInclude(course => course.Faculty)
                        .ThenInclude(faculty => faculty.Members.Where(member => member.Role == FacultyMembershipRole.Manager))
                            .ThenInclude(member => member.Staff)
                .SingleOrDefaultAsync(offering => offering.Name == name && offering.EndDate.Year == year);
        }
    }
}