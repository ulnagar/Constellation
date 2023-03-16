namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Families;
using System;

public sealed record StudentAddedToFamilyDomainEvent(
    Guid Id,
    StudentFamilyMembership Membership)
    : DomainEvent(Id);
