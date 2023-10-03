namespace Constellation.Infrastructure.Persistence.ConstellationContext.Interceptors;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Audit;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Primitives;
using Constellation.Infrastructure.Persistence.ConstellationContext.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class CreateAuditLogEntitiesInterceptor 
    : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateAuditLogEntitiesInterceptor(ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
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

        var auditEntries = new List<AuditEntry>();

        IEnumerable<EntityEntry<IFullyAuditableEntity>> entries =
            dbContext
                .ChangeTracker
                .Entries<IFullyAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.TypeName = entry.Entity.GetType().Name;
            auditEntry.UserId = _currentUserService.UserName;
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = entry.Entity.IsDeleted ? AuditType.Delete : AuditType.Update;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var entry in auditEntries)
        {
            dbContext
                .Set<Audit>()
                .Add(entry.ToAudit());
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
