namespace Constellation.Core.Models.SchoolContacts.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record SchoolContactRoleCreatedDomainEvent(
    DomainEventId Id,
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId)
    : DomainEvent(Id);