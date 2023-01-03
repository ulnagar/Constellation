namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class FacultyRepository : IFacultyRepository
{
    private readonly AppDbContext _dbContext;

    public FacultyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .AnyAsync(faculty => !faculty.IsDeleted && faculty.Name == name, cancellationToken);


    public void Insert(Faculty faculty) =>
        _dbContext
            .Set<Faculty>()
            .Add(faculty);
}
