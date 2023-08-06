namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    private readonly DateOnly _startOfYear;

    public LessonRepository(AppDbContext context)
    {
        _context = context;

        _startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);
    }

    public async Task<List<SciencePracLesson>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => lesson.DueDate > _startOfYear)
            .ToListAsync(cancellationToken);
        
    public async Task<List<SciencePracLesson>> GetAllCurrent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson => 
                lesson.DueDate > _startOfYear &&
                lesson.Rolls.Any(roll => roll.Status == LessonStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<List<SciencePracLesson>> GetAllForSchool(
        string SchoolCode, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<SciencePracLesson>()
            .Where(lesson =>
                lesson.DueDate > _startOfYear &&
                lesson.Rolls.Any(roll => roll.SchoolCode == SchoolCode))
            .ToListAsync(cancellationToken);

    public async Task<SciencePracLesson> GetById(
        SciencePracLessonId LessonId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SciencePracLesson>()
            .SingleOrDefaultAsync(lesson => lesson.Id == LessonId, cancellationToken);
}