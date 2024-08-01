namespace Constellation.Infrastructure.Persistence.ConstellationContext;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Students;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    public DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<TimetablePeriod> Periods { get; set; }
    public DbSet<JobActivation> JobActivations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace.Contains("ConstellationContext")); // Only include the local EntityConfigurations

        base.OnModelCreating(builder);
    }
}