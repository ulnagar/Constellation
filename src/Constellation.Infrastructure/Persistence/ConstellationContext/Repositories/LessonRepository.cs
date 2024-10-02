namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Abstractions.Clock;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public LessonRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<SciencePracLesson>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => lesson.DueDate > _dateTime.FirstDayOfYear)
            .ToListAsync(cancellationToken);
        
    public async Task<List<SciencePracLesson>> GetAllCurrent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.Rolls.Any(roll => roll.Status == LessonStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<List<SciencePracLesson>> GetAllForSchool(
        string SchoolCode, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson =>
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.Rolls.Any(roll => roll.SchoolCode == SchoolCode))
            .ToListAsync(cancellationToken);

    public async Task<List<SciencePracLesson>> GetAllForCourse(
        CourseId CourseId,
        CancellationToken cancellationToken = default)
    {
        var offeringIds = await _context
            .Set<Offering>()
            .Where(offering => offering.CourseId == CourseId)
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.Offerings.Any(record => 
                    offeringIds.Contains(record.OfferingId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SciencePracLesson>> GetAllForOffering(
        OfferingId OfferingId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.Offerings.Any(record => 
                    record.OfferingId == OfferingId))
            .ToListAsync(cancellationToken);
        
    public async Task<List<SciencePracLesson>> GetAllForStudent(
        StudentId studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.Rolls.Any(roll => 
                    roll.Attendance.Any(attendance => 
                        attendance.StudentId == studentId)))
            .ToListAsync(cancellationToken);

    public async Task<SciencePracLesson> GetById(
        SciencePracLessonId LessonId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .SingleOrDefaultAsync(lesson => lesson.Id == LessonId, cancellationToken);

    public async Task<List<SciencePracLesson>> GetAllWithOverdueRolls(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _dateTime.FirstDayOfYear &&
                lesson.DueDate < _dateTime.Today && 
                lesson.Rolls.Any(roll => roll.Status == LessonStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<List<SciencePracLesson>> GetWithoutPresentStudents(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson =>
                lesson.DueDate >= _dateTime.FirstDayOfYear &&
                lesson.Rolls.Any(roll =>
                    roll.Status == LessonStatus.Completed &&
                    roll.Attendance.All(entry => !entry.Present)))
            .ToListAsync(cancellationToken);

    public void Insert(SciencePracLesson lesson) => _context.Set<SciencePracLesson>().Add(lesson);

    public void Delete(SciencePracLesson lesson) => _context.Set<SciencePracLesson>().Remove(lesson);

    public void Delete(SciencePracAttendance attendance) => _context.Set<SciencePracAttendance>().Remove(attendance);
}