namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class CanvasOperationsRepository : ICanvasOperationsRepository
{
    private readonly AppDbContext _context;

    public CanvasOperationsRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CanvasOperation>> All(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasOperation>()
            .ToListAsync(cancellationToken);

    public async Task<List<CanvasOperation>> AllToProcess(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasOperation>()
            .Where(operation => 
                !operation.IsCompleted && 
                !operation.IsDeleted && 
                operation.ScheduledFor <= DateTime.Now)
            .ToListAsync(cancellationToken);

    public async Task<List<CanvasOperation>> AllWithFilter(
        Expression<Func<CanvasOperation, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasOperation>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

    public async Task<CanvasOperation> WithDetails(
        int id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasOperation>()
            .SingleOrDefaultAsync(operation => operation.Id == id, cancellationToken);

    public async Task<CanvasOperation> WithFilter(
        Expression<Func<CanvasOperation, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasOperation>()
            .SingleOrDefaultAsync(predicate, cancellationToken);

    public void Insert(CanvasOperation operation) =>
        _context.Set<CanvasOperation>().Add(operation);
}
