namespace Constellation.Application.Offerings.AddMultipleSessionsToOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using Core.Models.Timetables.Identifiers;
using System.Collections.Generic;

public sealed record AddMultipleSessionsToOfferingCommand(
        OfferingId OfferingId,
        List<PeriodId> PeriodIds)
    : ICommand;