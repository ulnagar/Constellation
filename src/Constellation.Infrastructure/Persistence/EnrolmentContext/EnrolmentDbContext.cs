namespace Constellation.Infrastructure.Persistence.EnrolmentContext;

using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Core.Models.Auth;
using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Primitives;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.EntityConfigurations.Outbox;
using System;
using System.Reflection;

public sealed class EnrolmentDbContext : IdentityDbContext<EnrolmentUser, IdentityRole<Guid>, Guid>
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
