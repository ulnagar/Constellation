namespace Constellation.Infrastructure.Persistence.ConstellationContext;

using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Converters;
using Core.Models.Auth;
using Core.Primitives;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Outbox;
using System.Reflection;

public class ConstellationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public const string TeamsOperationId = "TeamsOperationId";

    public ConstellationDbContext(DbContextOptions<ConstellationDbContext> options)
        : base(options)
    { }

    public DbSet<JobActivation> JobActivations { get; set; }

    [DbFunction("SOUNDEX", IsBuiltIn = true)]
    public static string Soundex(string query) => throw new NotImplementedException();

    [DbFunction("DIFFERENCE", IsBuiltIn = true)]
    public static int Difference(string s1, string s2) => throw new NotImplementedException();


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasSequence<int>(TeamsOperationId)
            .StartsAt(202_500)
            .IncrementsBy(1);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace.Contains("ConstellationContext")); // Only include the local EntityConfigurations

        base.OnModelCreating(builder);
    }

    public Task AddIntegrationEvent(IIntegrationEvent integrationEvent)
    {
        OutboxMessage eventMessage = new()
        {
            Id = Guid.NewGuid(),
            OccurredOn = integrationEvent.DelayUntil?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(
                integrationEvent,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })
        };

        Set<OutboxMessage>().Add(eventMessage);

        return Task.CompletedTask;
    }
}