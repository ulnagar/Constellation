#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
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

    public async Task<Course?> GetByOfferingId(
        OfferingId offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Course>()
            .Where(course => course.Offerings.Any(offering => offering.Id == offeringId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Course?> ForEditAsync(int id) =>
        await _context.Set<Course>()
            .SingleOrDefaultAsync(course => course.Id == id);

    public async Task<bool> AnyWithId(int id)
    {
        return await _context.Set<Course>()
            .AnyAsync(course => course.Id == id);
    }
}