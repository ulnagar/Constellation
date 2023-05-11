namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class AdobeConnectOperationsRepository : IAdobeConnectOperationsRepository
{
    private readonly AppDbContext _context;

    public AdobeConnectOperationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdobeConnectOperation>> GetByCoverId(
        ClassCoverId coverId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AdobeConnectOperation>()
            .Where(operation => operation.CoverId == coverId.Value)
            .ToListAsync(cancellationToken);

    private IQueryable<AdobeConnectOperation> Collection()
    {
        return _context.AdobeConnectOperations.Include(op => op.Room);
    }

    public AdobeConnectOperation WithDetails(int id)
    {
        return Collection()
            .SingleOrDefault(d => d.Id == id);
    }

    public AdobeConnectOperation WithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate)
    {
        return Collection()
            .FirstOrDefault(predicate);
    }

    public ICollection<AdobeConnectOperation> All()
    {
        return Collection()
            .ToList();
    }

    public ICollection<AdobeConnectOperation> AllWithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate)
    {
        return Collection()
            .Where(predicate)
            .ToList();
    }

    public async Task<ICollection<AdobeConnectOperation>> AllToProcess()
    {
        return await _context.AdobeConnectOperations
            .Include(operation => operation.Room)
            .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
            .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
            .Include(operation => ((TeacherAdobeConnectGroupOperation)operation).Teacher)
            .Where(operation => operation.DateScheduled == DateTime.Today &&
                operation.IsCompleted == false &&
                operation.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<ICollection<AdobeConnectOperation>> AllOverdue()
    {
        return await _context.AdobeConnectOperations
            .Include(operation => operation.Room)
            .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
            .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
            .Include(operation => ((TeacherAdobeConnectGroupOperation)operation).Teacher)
            .Where(operation => operation.DateScheduled < DateTime.Today &&
                operation.IsCompleted == false &&
                operation.IsDeleted == false)
            .ToListAsync();
    }

    public AdobeConnectOperationsList AllRecent()
    {
        var searchDate = DateTime.Now.Date.AddDays(-7);

        var dataset = new AdobeConnectOperationsList
        {
            StudentOperations = Collection().OfType<StudentAdobeConnectOperation>().Include(op => op.Student).Include(op => op.Room)
                .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                .ToList(),
            TeacherOperations = Collection().OfType<TeacherAdobeConnectOperation>().Include(op => op.Teacher).Include(op => op.Room)
                .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                .ToList(),
            CasualOperations = Collection().OfType<CasualAdobeConnectOperation>().Include(op => op.Room)
                .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                .ToList(),
            TeacherGroupOperations = Collection().OfType<TeacherAdobeConnectGroupOperation>().Include(op => op.Teacher)
                .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                .ToList()
        };

        return dataset;
    }

    public async Task<ICollection<AdobeConnectOperation>> AllRecentAsync()
    {
        var searchDate = DateTime.Now.Date.AddDays(-7);

        var operations = await _context.AdobeConnectOperations
            .Include(operation => operation.Room)
            .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
            .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
            .Include(operation => ((TeacherAdobeConnectGroupOperation)operation).Teacher)
            .Where(operation => (operation.DateScheduled > searchDate || operation.IsCompleted == false) && operation.IsDeleted == false)
            .ToListAsync();

        return operations;
    }

    public async Task<AdobeConnectOperation> ForProcessingAsync(int id)
    {
        return await _context.AdobeConnectOperations
            .Include(operation => operation.Room)
            .SingleOrDefaultAsync(operation => operation.Id == id);
    }

    public void Insert(AdobeConnectOperation operation) =>
        _context.Set<AdobeConnectOperation>().Add(operation);
}