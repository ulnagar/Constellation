namespace Constellation.Infrastructure.Persistence.ConstellationContext;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Persistence.ConstellationContext.ContextExtensions;
using Constellation.Infrastructure.Persistence.ConstellationContext.ContextSets;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using System.Reflection;

public class AppDbContext : KeyApiAuthorizationDbContext<AppUser, AppRole, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions)
    {
        MandatoryTraining = new MandatoryTrainingSets(this);
    }

    public DbSet<AdobeConnectOperation> AdobeConnectOperations { get; set; }
    public DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    public DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<SchoolContact> SchoolContacts { get; set; }
    public DbSet<SchoolContactRole> SchoolContactRoles { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<TimetablePeriod> Periods { get; set; }
    public DbSet<AdobeConnectRoom> Rooms { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceNotes> DeviceNotes { get; set; }
    public DbSet<DeviceAllocation> DeviceAllocations { get; set; }
    public DbSet<StudentWholeAbsence> WholeAbsences { get; set; }
    public DbSet<StudentPartialAbsence> PartialAbsences { get; set; }
    public DbSet<CanvasOperation> CanvasOperations { get; set; }
    public DbSet<JobActivation> JobActivations { get; set; }
    public DbSet<StocktakeEvent> StocktakeEvents { get; set; }
    public DbSet<StocktakeSighting> StocktakeSightings { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public IMandatoryTrainingSets MandatoryTraining { get; private set; }
    public DbSet<TrainingModule> MandatoryTraining_Modules { get; set; }
    public DbSet<TrainingCompletion> MandatoryTraining_CompletionRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace.Contains("ConstellationContext")); // Only include the local EntityConfigurations

        base.OnModelCreating(builder);
    }

    public override EntityEntry Add(object entity) => base.Add(entity);
}