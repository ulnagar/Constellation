using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class CourseOfferingRepository : ICourseOfferingRepository
    {
        private readonly AppDbContext _context;

        public CourseOfferingRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Offering> Collection()
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

        public async Task<Offering?> GetById(
            OfferingId offeringId, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Offering>()
                .Include(offering => offering.Enrolments)
                .Where(offering => offering.Id == offeringId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<Offering>> GetAllActive(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Offering>()
                .Where(offering => 
                    offering.StartDate <= DateTime.Now && 
                    offering.EndDate >= DateTime.Now &&
                    offering.Sessions.Any(session => !session.IsDeleted))
                .OrderBy(offering => offering.Course.Grade)
                .ThenBy(offering => offering.Name)
                .ToListAsync(cancellationToken);

        public async Task<List<Offering>> GetByCourseId(
            int courseId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Offering>()
                .Include(offering => offering.Sessions)
                .Where(offering => offering.CourseId == courseId)
                .ToListAsync(cancellationToken);

        public async Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
            string studentId,
            DateTime AbsenceDate,
            int DayNumber,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Enrolment>()
                .Where(enrolment => enrolment.StudentId == studentId &&
                    // enrolment was created before the absence date
                    enrolment.DateCreated < AbsenceDate &&
                    // enrolment is either still current (not deleted) OR was deleted after the absence date
                    (!enrolment.IsDeleted || enrolment.DateDeleted.Value.Date > AbsenceDate) &&
                    // offering ends after the absence date
                    enrolment.Offering.EndDate > AbsenceDate &&
                    enrolment.Offering.Sessions.Any(session =>
                        // session was created before the absence date
                        session.DateCreated < AbsenceDate &&
                        // session is either still current (not deleted) OR was deleted after the absence date
                        (!session.IsDeleted || session.DateDeleted.Value.Date > AbsenceDate) &&
                        // session is for the same day as the absence
                        session.Period.Day == DayNumber))
                .Select(enrolment => enrolment.Offering)
                .Distinct()
                .ToListAsync(cancellationToken);

        // Method is not async as we are passing the task to another method
        public Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
            string studentId,
            DateOnly AbsenceDate,
            int DayNumber,
            CancellationToken cancellationToken = default) =>
            GetCurrentEnrolmentsFromStudentForDate(studentId, AbsenceDate.ToDateTime(TimeOnly.MinValue), DayNumber, cancellationToken);

        public async Task<List<Offering>> GetByStudentId(
            string studentId, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Enrolment>()
                .Where(enrolment => enrolment.StudentId == studentId &&
                    !enrolment.IsDeleted)
            .Select(enrolment => enrolment.Offering)
            .Distinct()
            .ToListAsync(cancellationToken);

        public Offering WithDetails(OfferingId id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public Offering WithFilter(Expression<Func<Offering, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public async Task<Offering> GetForExistCheck(OfferingId id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions) // Required for CoverService and EnrolmentService to create operations
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public ICollection<Offering> All()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .ToList();
        }

        public ICollection<Offering> AllWithFilter(Expression<Func<Offering, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<Offering> AllCurrentOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now)
                .ToList();
        }

        public ICollection<Offering> AllFutureOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.StartDate > DateTime.Now)
                .ToList();
        }

        public ICollection<Offering> AllPastOfferings()
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate < DateTime.Now)
                .ToList();
        }

        public ICollection<Offering> AllFromFaculty(Faculty faculty)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Course.Faculty == faculty)
                .ToList();
        }

        public ICollection<Offering> AllFromGrade(Grade grade)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Course.Grade == grade)
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now || o.StartDate > DateTime.Now)
                .ToList();
        }

        public ICollection<Offering> AllForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<Offering> AllCurrentAndFutureForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate > DateTime.Today)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<Offering> AllCurrentForStudent(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.EndDate > DateTime.Today && o.StartDate < DateTime.Today)
                .Where(o => o.Enrolments.Any(e => e.Student.StudentId == id && e.IsDeleted == false))
                .ToList();
        }

        public ICollection<Offering> AllForTeacher(string id)
        {
            return Collection()
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Sessions.Any(s => s.StaffId == id && !s.IsDeleted))
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now)
                .ToList();
        }

        public async Task<ICollection<Offering>> AllForTeacherAsync(string id)
        {
            return await _context.Offerings
                .Where(offering => offering.Sessions.Any(s => s.StaffId == id && !s.IsDeleted) && offering.StartDate < DateTime.Now && offering.EndDate > DateTime.Now)
                .OrderBy(offering => offering.Name)
                .ThenBy(offering => offering.StartDate)
                .ToListAsync();
        }

        public ICollection<Staff> AllClassTeachers(OfferingId id)
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

        public async Task<ICollection<Offering>> ForSelectionAsync()
        {
            return await _context.Offerings
                .Where(offering => offering.EndDate > DateTime.Now && offering.StartDate < DateTime.Now)
                .ToListAsync();
        }

        public async Task<ICollection<Offering>> FromGradeForBulkEnrolAsync(Grade grade)
        {
            return await _context.Offerings
                .Include(offering => offering.Course)
                .Where(offering => offering.Course.Grade == grade && offering.EndDate >= DateTime.Now)
                .ToListAsync();
        }

        public async Task<Offering> ForEnrolmentAsync(OfferingId id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<ICollection<Offering>> ForListAsync(Expression<Func<Offering, bool>> predicate)
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

        public async Task<Offering> ForDetailDisplayAsync(OfferingId id)
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
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<Offering> ForEditAsync(OfferingId id)
        {
            return await _context.Offerings
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<Offering> ForSessionEditAsync(OfferingId id)
        {
            return await _context.Offerings
                .Include(offering => offering.Sessions)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<Offering> ForRollCreationAsync(OfferingId id)
        {
            return await _context.Offerings
                .Include(offering => offering.Enrolments)
                .ThenInclude(enrolment => enrolment.Student)
                .ThenInclude(student => student.School)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<Offering> ForCoverCreationAsync(OfferingId id)
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

        public async Task<ICollection<Staff>> AllTeachersForCoverCreationAsync(OfferingId id)
        {
            var offering = await _context.Offerings
                .Include(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleAsync(offering => offering.Id == id);

            var teachers = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher);

            return teachers.ToList();
        }

        public async Task<Offering> ForSessionCreationAsync(OfferingId id)
        {
            return await _context.Offerings
                .Include(offering => offering.Enrolments)
                .ThenInclude(enrolment => enrolment.Student)
                .SingleOrDefaultAsync(offering => offering.Id == id);
        }

        public async Task<bool> AnyWithId(OfferingId id)
        {
            return await _context.Offerings
                .AnyAsync(offering => offering.Id == id);
        }

        public async Task<ICollection<Offering>> ForTeacherAndDates(string staffId, ICollection<int> dates)
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

        public async Task<Offering> GetFromYearAndName(int year, string name)
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