namespace Constellation.Core.Models.EnrolmentContext.Audit;

using Enum;
using System;

public sealed class AuditLog
{
    public int Id { get; private set; }
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required string PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public required AuditAction Action { get; init; }
    public required string ChangedBy { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string? CorrelationId { get; init; }
}