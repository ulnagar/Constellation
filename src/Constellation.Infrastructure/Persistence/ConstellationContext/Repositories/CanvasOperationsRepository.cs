namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class CanvasOperationsRepository : ICanvasOperationsRepository
{
    private readonly AppDbContext _context;

    public CanvasOperationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<CanvasOperation>> All()
    {
        return await _context.CanvasOperations
            .ToListAsync();
    }

    public async Task<ICollection<CanvasOperation>> AllToProcess()
    {
        return await _context.CanvasOperations
            .Where(operation => !operation.IsCompleted && !operation.IsDeleted && operation.ScheduledFor <= DateTime.Now)
            .ToListAsync();
    }

    public async Task<ICollection<CanvasOperation>> AllWithFilter(Expression<Func<CanvasOperation, bool>> predicate)
    {
        return await _context.CanvasOperations
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<CanvasOperation> WithDetails(int id)
    {
        return await _context.CanvasOperations
            .FirstAsync(operation => operation.Id == id);
    }

    public async Task<CanvasOperation> WithFilter(Expression<Func<CanvasOperation, bool>> predicate)
    {
        return await _context.CanvasOperations
            .FirstOrDefaultAsync(predicate);
    }

    public void Insert(CanvasOperation operation) =>
        _context.Set<CanvasOperation>().Add(operation);
}
