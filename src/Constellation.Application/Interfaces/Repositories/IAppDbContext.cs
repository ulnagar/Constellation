namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Students;
using Core.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public interface IAppDbContext
{
    DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    DbSet<JobActivation> JobActivations { get; set; }
    DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    DbSet<TimetablePeriod> Periods { get; set; }
    DbSet<School> Schools { get; set; }
    DbSet<Staff> Staff { get; set; }
    DbSet<Student> Students { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task AddIntegrationEvent(IIntegrationEvent integrationEvent);
}