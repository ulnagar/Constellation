#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public CourseRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
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

    public async Task<Course?> GetByLessonId(
        SciencePracLessonId lessonId,
        CancellationToken cancellationToken = default)
    {
        var record = await _context
            .Set<SciencePracLesson>()
            .Where(lesson => lesson.Id == lessonId)
            .Select(lesson => lesson.Offerings.First())
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null)
            return null;

        return await _context
            .Set<Course>()
            .Where(course => course.Offerings.Any(offering => offering.Id == record.OfferingId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Course> Collection()
    {
        return _context.Set<Course>()
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
            .Where(c => c.Offerings.Any(o => o.EndDate >= _dateTime.Today))
            .ToList();
    }

    public ICollection<Course> AllWithoutActiveOfferings()
    {
        return Collection()
            .Where(c => !c.Offerings.Any(o => o.EndDate >= _dateTime.Today))
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
        return await _context.Set<Course>()
            .Include(course => course.Offerings)
            .SingleOrDefaultAsync(course => course.Id == courseId);
    }

    public async Task<ICollection<Course>> ForListAsync(Expression<Func<Course, bool>> predicate)
    {
        return await _context.Set<Course>()
            .Include(course => course.Offerings)
            .Include(course => course.Faculty)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Course> ForDetailDisplayAsync(int id)
    {
        return await _context.Set<Course>()
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
        return await _context.Set<Course>()
            .SingleOrDefaultAsync(course => course.Id == id);
    }

    public async Task<ICollection<Course>> ForSelectionAsync()
    {
        return await _context.Set<Course>()
            .Include(course => course.Faculty)
            .OrderBy(course => course.Grade)
            .ThenBy(course => course.Name)
            .ToListAsync();
    }

    public async Task<bool> AnyWithId(int id)
    {
        return await _context.Set<Course>()
            .AnyAsync(course => course.Id == id);
    }
}