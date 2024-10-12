﻿namespace Constellation.Core.Models.Students.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record StudentWithdrawnDomainEvent(
    DomainEventId Id,
    StudentId StudentId)
    : DomainEvent(Id);