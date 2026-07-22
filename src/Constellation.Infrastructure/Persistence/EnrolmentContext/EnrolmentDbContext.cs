namespace Constellation.Infrastructure.Persistence.EnrolmentContext;

using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Microsoft.EntityFrameworkCore;
using Shared.EntityConfigurations.Outbox;
using System;
using System.Reflection;

public sealed class EnrolmentDbContext : DbContext
{
    public EnrolmentDbContext(
        DbContextOptions<EnrolmentDbContext> options)
        : base(options)
    { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace?.Contains("EnrolmentContext", StringComparison.InvariantCultureIgnoreCase) ?? false);


        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        //modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
