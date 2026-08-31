namespace Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Primitives;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

public sealed class UpdateAuditableEntitiesInterceptor 
    : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;


    public UpdateAuditableEntitiesInterceptor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ICurrentUserService currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        DbContext? dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(
                eventData, 
                result, 
                cancellationToken);
        }

        IEnumerable<EntityEntry<IAuditableEntity>> entries =
            dbContext
                .ChangeTracker
                .Entries<IAuditableEntity>();

        foreach (EntityEntry<IAuditableEntity> entityEntry in entries)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.CreatedBy = currentUserService.UserName;
                    entityEntry.Entity.CreatedAt = dateTimeProvider.Now;
                    break;
                case EntityState.Modified:
                    if (entityEntry.Entity.IsDeleted)
                    {
                        entityEntry.Entity.DeletedBy = currentUserService.UserName;
                        entityEntry.Entity.DeletedAt = dateTimeProvider.Now;
                    }
                    entityEntry.Entity.ModifiedBy = currentUserService.UserName;
                    entityEntry.Entity.ModifiedAt = dateTimeProvider.Now;
                    break;
            }
        }


        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
