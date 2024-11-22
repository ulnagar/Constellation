namespace Constellation.Core.Models.Reports.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record AcademicReportCreatedDomainEvent(
    DomainEventId Id,
    AcademicReportId ReportId)
    : DomainEvent(Id);