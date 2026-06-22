namespace Constellation.Infrastructure.Persistence.EnrolmentContext;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public sealed class EnrolmentDbContext : DbContext
{
    public EnrolmentDbContext(
        DbContextOptions<EnrolmentDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.GetTypeInfo().Namespace?.Contains("EnrolmentContext", StringComparison.InvariantCultureIgnoreCase) ?? false);

        base.OnModelCreating(modelBuilder);
    }
}
