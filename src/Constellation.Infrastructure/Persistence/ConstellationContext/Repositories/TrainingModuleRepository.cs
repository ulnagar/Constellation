﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class TrainingModuleRepository
    : ITrainingModuleRepository
{
    private readonly AppDbContext _context;

    public TrainingModuleRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrainingModule>> GetAllModules(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<TrainingModule>()
            .ToListAsync(cancellationToken);

    public async Task<TrainingModule> GetModuleByName(
        string name, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<TrainingModule>()
            .Where(module => module.Name == name)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TrainingModule> GetModuleById(
        TrainingModuleId moduleId, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<TrainingModule>()
            .Where(module => module.Id == moduleId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<TrainingModule>> GetModulesByAssignee(
        StaffId staffId, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<TrainingModule>()
            .Where(module => 
                module.Assignees.Any(assignee => 
                    assignee.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public void Insert(TrainingModule module) => _context.Set<TrainingModule>().Add(module);
}