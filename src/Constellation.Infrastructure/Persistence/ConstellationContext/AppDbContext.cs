namespace Constellation.Infrastructure.Persistence.ConstellationContext;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Persistence.ConstellationContext.ContextExtensions;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

public class AppDbContext : KeyApiAuthorizationDbContext<AppUser, AppRole, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions)
    { }

    public DbSet<AdobeConnectOperation> AdobeConnectOperations { get; set; }
    public DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    public DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<TimetablePeriod> Periods { get; set; }
    public DbSet<AdobeConnectRoom> Rooms { get; set; }
    public DbSet<JobActivation> JobActivations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace.Contains("ConstellationContext")); // Only include the local EntityConfigurations

        base.OnModelCreating(builder);
    }
}