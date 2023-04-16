namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record AcademicReportCreatedDomainEvent(
    DomainEventId Id,
    AcademicReportId ReportId)
    : DomainEvent(Id);