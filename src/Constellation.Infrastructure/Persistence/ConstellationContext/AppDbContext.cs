namespace Constellation.Infrastructure.Persistence.ConstellationContext;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Core.Primitives;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Outbox;
using System.Reflection;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    public DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    public DbSet<JobActivation> JobActivations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
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