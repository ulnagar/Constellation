namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.StaffMembers.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public IMSTeamOperationsRepository MSTeamOperations { get; set; }
    public ISchoolRepository Schools { get; set; }
    public IStaffRepository Staff { get; set; }
    public IStudentRepository Students { get; set; }
    public ITimetablePeriodRepository Periods { get; set; }

    public UnitOfWork(
        AppDbContext context, 
        IDateTimeProvider dateTime)
    {
        _context = context;

        MSTeamOperations = new MSTeamOperationsRepository(context);
        Schools = new SchoolRepository(context);
        Staff = new StaffRepository(context);
        Students = new StudentRepository(context, dateTime);
        Periods = new TimetablePeriodRepository(context);
    }
    
    public void Add<TEntity>(TEntity entity) where TEntity : class => _context.Set<TEntity>().Add(entity);

    public async Task CompleteAsync(CancellationToken token) => await _context.SaveChangesAsync(token);

    public async Task CompleteAsync() => await _context.SaveChangesAsync();
}
