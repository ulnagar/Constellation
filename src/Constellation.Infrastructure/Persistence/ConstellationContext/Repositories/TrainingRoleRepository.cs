namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;

internal sealed class TrainingRoleRepository
    : ITrainingRoleRepository
{
    private readonly AppDbContext _context;

    public TrainingRoleRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingRole> GetRoleByName(
        string name,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingRole>()
            .Where(role => role.Name == name)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TrainingRole> GetRoleById(
        TrainingRoleId roleId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingRole>()
            .Where(role => role.Id == roleId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<TrainingRole>> GetAllRoles(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingRole>()
            .ToListAsync(cancellationToken);

    public async Task<List<TrainingRole>> GetRolesForStaffMember(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingRole>()
            .Where(role => role.Members.Any(member => member.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<List<TrainingRole>> GetRolesForModule(
        TrainingModuleId moduleId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingRole>()
            .Where(role => role.Modules.Any(module => module.ModuleId == moduleId))
            .ToListAsync(cancellationToken);

    public void Insert(TrainingRole role) => _context.Set<TrainingRole>().Add(role);

}