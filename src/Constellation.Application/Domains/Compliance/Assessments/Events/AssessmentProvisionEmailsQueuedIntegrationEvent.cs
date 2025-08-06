namespace Constellation.Application.Domains.Compliance.Assessments.Events;

using Core.IntegrationEvents;
using Core.Models.Identifiers;
using Core.ValueObjects;

public sealed record AssessmentProvisionEmailsQueuedIntegrationEvent(
    IntegrationEventId Id,
    Name RequesterName,
    EmailAddress RequesterEmailAddress)
    : IntegrationEvent(Id);
