﻿namespace Constellation.Core.Models.Enrolments.Events;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId,
    StudentId StudentId,
    OfferingId OfferingId)
    : DomainEvent(Id);