using Constellation.Core.Abstractions;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

internal class TrainingCompletionRepository : ITrainingCompletionRepository
{
    private readonly AppDbContext _context;

    public TrainingCompletionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrainingCompletion>> GetCurrentForStaffMember(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingCompletion>()
            .Where(record => !record.IsDeleted && record.StaffId == staffId)
            .ToListAsync(cancellationToken);

    public async Task<bool> AnyExistingRecordForTeacherAndDate(
        string staffId, 
        TrainingModuleId moduleId, 
        DateTime completedDate, 
        bool notRequired, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingCompletion>()
            .AnyAsync(record =>
                record.StaffId == staffId &&
                record.TrainingModuleId == moduleId &&
                (record.CompletedDate == completedDate || (record.NotRequired && notRequired)),
            cancellationToken);

    public async Task<List<TrainingCompletion>> GetForModule(
        TrainingModuleId moduleId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingCompletion>()
            .Where(record => record.TrainingModuleId == moduleId)
            .ToListAsync(cancellationToken);

    public async Task<TrainingCompletion> GetById(
        TrainingCompletionId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingCompletion>()
            .Where(record => record.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(TrainingCompletion record) =>
        _context.Set<TrainingCompletion>().Add(record);
}
