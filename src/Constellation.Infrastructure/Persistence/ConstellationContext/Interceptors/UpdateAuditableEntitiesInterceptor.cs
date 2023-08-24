namespace Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class UpdateAuditableEntitiesInterceptor 
    : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateAuditableEntitiesInterceptor(ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
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
                    entityEntry.Entity.CreatedBy = _currentUserService.UserName;
                    entityEntry.Entity.CreatedAt = _dateTimeProvider.Now;
                    break;
                case EntityState.Modified:
                    if (entityEntry.Entity.IsDeleted)
                    {
                        entityEntry.Entity.DeletedBy = _currentUserService.UserName;
                        entityEntry.Entity.DeletedAt = _dateTimeProvider.Now;
                    }
                    entityEntry.Entity.ModifiedBy = _currentUserService.UserName;
                    entityEntry.Entity.ModifiedAt = _dateTimeProvider.Now;
                    break;
            }
        }


        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
