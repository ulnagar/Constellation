namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Families;
using System;

public sealed record StudentResidentialFamilyChangedDomainEvent(
    Guid Id,
    StudentFamilyMembership Membership)
    : DomainEvent(Id);