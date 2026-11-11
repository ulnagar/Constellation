namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentParent.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

internal sealed class EnrolmentParentRepository : IEnrolmentParentRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentParentRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<EnrolmentParent?> GetById(
        EnrolmentParentId parentId, 
        CancellationToken cancellationToken = default) =>
    await _context
        .Set<EnrolmentParent>()
        .FirstOrDefaultAsync(p => p.Id == parentId, cancellationToken);

    public async Task<EnrolmentParent?> GetByEmail(
        EmailAddress email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentParent>()
            .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);

    public async Task<List<EnrolmentParent>> GetListFromIds(
        List<EnrolmentParentId> parentIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentParent>()
            .Where(p => parentIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

    public void Insert(EnrolmentParent parent) => _context.Set<EnrolmentParent>().Add(parent);
    
    public void Remove(EnrolmentParent parent) => _context.Set<EnrolmentParent>().Remove(parent);
}
