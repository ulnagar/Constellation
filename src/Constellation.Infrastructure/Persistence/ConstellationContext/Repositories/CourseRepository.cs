#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetById(
        int courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Course>()
            .FirstOrDefaultAsync(course => course.Id == courseId, cancellationToken);

    public async Task<List<Course>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Course>()
            .ToListAsync(cancellationToken);

    private IQueryable<Course> Collection()
    {
        return _context.Courses
            .Include(course => course.Offerings)
                .ThenInclude(offering => offering.Enrolments)
                    .ThenInclude(enrolment => enrolment.Student)
            .Include(course => course.Offerings)
                .ThenInclude(offering => offering.Sessions)
                    .ThenInclude(session => session.Room)
            .Include(course => course.Offerings)
                .ThenInclude(offering => offering.Sessions)
                    .ThenInclude(session => session.Period)
            .Include(course => course.Offerings)
                .ThenInclude(offering => offering.Sessions)
                    .ThenInclude(session => session.Teacher)
            .Include(course => course.Faculty)
                .ThenInclude(faculty => faculty.Members.Where(member => member.Role == FacultyMembershipRole.Manager))
                    .ThenInclude(member => member.Staff);
    }

    public Course WithDetails(int id)
    {
        return Collection()
            .SingleOrDefault(d => d.Id == id);
    }

    public Course WithFilter(Expression<Func<Course, bool>> predicate)
    {
        return Collection()
            .FirstOrDefault(predicate);
    }

    public ICollection<Course> All()
    {
        return Collection()
            .ToList();
    }

    public ICollection<Course> AllWithFilter(Expression<Func<Course, bool>> predicate)
    {
        return Collection()
            .Where(predicate)
            .ToList();
    }

    public ICollection<Course> AllFromFaculty(Guid facultyId)
    {
        return Collection()
            .Where(c => c.FacultyId == facultyId)
            .ToList();
    }

    public ICollection<Course> AllFromGrade(Grade grade)
    {
        return Collection()
            .Where(c => c.Grade == grade)
            .ToList();
    }

    public ICollection<Course> AllWithActiveOfferings()
    {
        return Collection()
            .Where(c => c.Offerings.Any(o => o.EndDate > DateTime.Now))
            .ToList();
    }

    public ICollection<Course> AllWithoutActiveOfferings()
    {
        return Collection()
            .Where(c => !c.Offerings.Any(o => o.EndDate > DateTime.Now))
            .ToList();
    }

    //public async Task<IDictionary<int, string>> AllForLessonsPortal()
    //{
    //    var courses = await _context.Courses
    //        .Include(course => course.Offerings)
    //            .ThenInclude(offering => offering.Sessions)
    //        .Where(course => course.Faculty == Faculty.Science)
    //        .OrderBy(course => course.Name)
    //        .ToListAsync();

    //    var currentCourses = courses.Where(course => course.Offerings.Any(offering => offering.IsCurrent())).ToList();

    //    var dict = new Dictionary<int, string>();
    //    foreach (var course in currentCourses)
    //    {
    //        dict.Add(course.Id, $"{course.Grade} {course.Name}");
    //    }

    //    return dict;
    //}

    public async Task<Course> WithOfferingsForLessonsPortal(int courseId)
    {
        return await _context.Courses
            .Include(course => course.Offerings)
            .SingleOrDefaultAsync(course => course.Id == courseId);
    }

    public async Task<ICollection<Course>> ForListAsync(Expression<Func<Course, bool>> predicate)
    {
        return await _context.Courses
            .Include(course => course.Offerings)
            .Include(course => course.Faculty)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Course> ForDetailDisplayAsync(int id)
    {
        return await _context.Courses
            .Include(course => course.Faculty)
                .ThenInclude(faculty => faculty.Members.Where(member => member.Role == FacultyMembershipRole.Manager && !member.IsDeleted))
                    .ThenInclude(member => member.Staff)
            .Include(course => course.Offerings)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Teacher)
            .Include(course => course.Offerings)
            .ThenInclude(offering => offering.Enrolments)
            .SingleOrDefaultAsync(course => course.Id == id);
    }

    public async Task<Course> ForEditAsync(int id)
    {
        return await _context.Courses
            .SingleOrDefaultAsync(course => course.Id == id);
    }

    public async Task<ICollection<Course>> ForSelectionAsync()
    {
        return await _context.Courses
            .Include(course => course.Faculty)
            .OrderBy(course => course.Grade)
            .ThenBy(course => course.Name)
            .ToListAsync();
    }

    public async Task<bool> AnyWithId(int id)
    {
        return await _context.Courses
            .AnyAsync(course => course.Id == id);
    }
}