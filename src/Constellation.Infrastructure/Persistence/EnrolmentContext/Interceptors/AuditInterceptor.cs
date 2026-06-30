namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Interceptors;

using Constellation.Core.Models.EnrolmentContext.Audit;
using Constellation.Core.Models.EnrolmentContext.Audit.Enum;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _http;
    private List<PendingAuditEntry> _pending = [];
    private bool _auditSaveInProgress;

    public AuditInterceptor(ICurrentUserService currentUser, IHttpContextAccessor http)
    {
        _currentUser = currentUser;
        _http = http;
    }

    // ── Sync path ────────────────────────────────────────────────────────────

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!_auditSaveInProgress)
            _pending = Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData, int result)
    {
        if (!_auditSaveInProgress)
            Flush(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    // ── Async path ───────────────────────────────────────────────────────────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (!_auditSaveInProgress)
            _pending = Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (!_auditSaveInProgress)
            await FlushAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // ── Capture (runs before the save) ───────────────────────────────────────
    private List<PendingAuditEntry> Capture(DbContext? context)
    {
        if (context is null) return [];

        string? correlationId = _http.HttpContext?.TraceIdentifier;
        string changedBy = _currentUser.UserName;
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        List<PendingAuditEntry> entries = [];

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            AuditAction action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Modified,
                EntityState.Deleted => AuditAction.Deleted,
                _ => throw new UnreachableException()
            };

            // Owned entities (OwnsOne/OwnsMany) get logged under the OWNER's entity name,
            // with the navigation property name as a field prefix — otherwise every owned
            // "Name" on every entity in the system shows up identically as EntityName="Name".
            IForeignKey? ownership = entry.Metadata.FindOwnership();
            string entityName = ownership is not null
                ? ownership.PrincipalEntityType.ClrType.Name
                : entry.Entity.GetType().Name;
            string fieldPrefix = ownership?.PrincipalToDependent?.Name is { } navName
                ? navName + "."
                : string.Empty;

            // Shadow FK properties that exist purely to link an owned entity back to its
            // owner (shared PK) aren't meaningful field data — skip them entirely.
            HashSet<string> ownershipKeyProps = ownership is not null
                ? ownership.Properties.Select(p => p.Name).ToHashSet()
                : [];

            PendingAuditEntry pending = new(entry, entityName, action, changedBy, timestamp, correlationId);

            foreach (PropertyEntry prop in entry.Properties)
            {
                if (ownershipKeyProps.Contains(prop.Metadata.Name))
                    continue;

                string fieldName = fieldPrefix + prop.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (prop.Metadata.IsKey() && prop.IsTemporary)
                        {
                            pending.HasTemporaryKey = true;
                            continue;
                        }
                        pending.Changes.Add((fieldName, null, Format(prop.CurrentValue)));
                        break;

                    case EntityState.Modified:
                        if (!prop.IsModified) continue;
                        pending.Changes.Add((fieldName, Format(prop.OriginalValue), Format(prop.CurrentValue)));
                        break;

                    case EntityState.Deleted:
                        pending.Changes.Add((fieldName, Format(prop.OriginalValue), null));
                        break;
                }
            }

            CaptureComplexProperties(entry.ComplexProperties, entry.State, pending, fieldPrefix);

            if (pending.Changes.Count > 0)
                entries.Add(pending);
        }

        return entries;
    }

    private static void CaptureComplexProperties(
        IEnumerable<ComplexPropertyEntry> complexProperties,
        EntityState state,
        PendingAuditEntry pending,
        string prefix = "")
    {
        foreach (ComplexPropertyEntry complexProp in complexProperties)
        {
            string qualifiedName = prefix + complexProp.Metadata.Name;

            foreach (PropertyEntry prop in complexProp.Properties)
            {
                string fieldName = $"{qualifiedName}.{prop.Metadata.Name}";

                switch (state)
                {
                    case EntityState.Added:
                        pending.Changes.Add((fieldName, null, Format(prop.CurrentValue)));
                        break;

                    case EntityState.Modified:
                        if (!prop.IsModified) continue;
                        pending.Changes.Add((fieldName, Format(prop.OriginalValue), Format(prop.CurrentValue)));
                        break;

                    case EntityState.Deleted:
                        pending.Changes.Add((fieldName, Format(prop.OriginalValue), null));
                        break;
                }
            }

            // Recurse for nested complex types
            CaptureComplexProperties(complexProp.ComplexProperties, state, pending, qualifiedName + ".");
        }
    }

    // ── Flush (runs after the save) ──────────────────────────────────────────

    private void Flush(DbContext? context)
    {
        if (context is null || _pending.Count == 0) return;

        context.Set<AuditLog>().AddRange(Materialize(_pending));
        _auditSaveInProgress = true;
        try { context.SaveChanges(); }
        finally { _auditSaveInProgress = false; _pending.Clear(); }
    }

    private async Task FlushAsync(DbContext? context, CancellationToken ct = default)
    {
        if (context is null || _pending.Count == 0) return;

        context.Set<AuditLog>().AddRange(Materialize(_pending));
        _auditSaveInProgress = true;
        try { await context.SaveChangesAsync(ct); }
        finally { _auditSaveInProgress = false; _pending.Clear(); }
    }

    private static IEnumerable<AuditLog> Materialize(List<PendingAuditEntry> pending)
    {
        foreach (PendingAuditEntry entry in pending)
        {
            // For Added entities with db-generated keys, the real value is now available
            string entityId = entry.Entry.Properties
                .Where(p => p.Metadata.IsKey())
                .Select(p => Format(p.CurrentValue))
                .FirstOrDefault() ?? string.Empty;

            foreach ((string property, string? oldValue, string? newValue) in entry.Changes)
            {
                yield return new AuditLog
                {
                    EntityName = entry.EntityName,
                    EntityId = entityId,
                    PropertyName = property,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Action = entry.Action,
                    ChangedBy = entry.ChangedBy,
                    Timestamp = entry.Timestamp,
                    CorrelationId = entry.CorrelationId,
                };
            }
        }
    }

    private static string? Format(object? value) =>
        value is null or DBNull ? null : Convert.ToString(value, CultureInfo.InvariantCulture);

    // ── Private helper ───────────────────────────────────────────────────────

    private sealed class PendingAuditEntry(
        EntityEntry entry, string entityName, AuditAction action,
        string changedBy, DateTimeOffset timestamp, string? correlationId)
    {
        public EntityEntry Entry { get; } = entry;
        public string EntityName { get; } = entityName;
        public AuditAction Action { get; } = action;
        public string ChangedBy { get; } = changedBy;
        public DateTimeOffset Timestamp { get; } = timestamp;
        public string? CorrelationId { get; } = correlationId;
        public bool HasTemporaryKey { get; set; }
        public List<(string Property, string? OldValue, string? NewValue)> Changes { get; } = [];
    }
}