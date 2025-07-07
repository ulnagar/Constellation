namespace Constellation.Core.Models.StaffMembers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using System;

public sealed record StaffMemberMovedSchoolsDomainEvent(
    DomainEventId Id,
    StaffId StaffId,
    string PreviousSchoolCode,
    string CurrentSchoolCode,
    DateOnly? DelayUntil = null)
    : DomainEvent(Id, DelayUntil);