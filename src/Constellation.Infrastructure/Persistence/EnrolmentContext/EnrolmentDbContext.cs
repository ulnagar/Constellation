namespace Constellation.Infrastructure.Persistence.EnrolmentContext;

using Constellation.Core.Enums;
using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Primitives;
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
        configurationBuilder.Properties<Grade>().HaveConversion<GradeConverter>();

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace?.Contains("EnrolmentContext", StringComparison.InvariantCultureIgnoreCase) ?? false);


        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
