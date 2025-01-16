namespace Constellation.Application.Offerings.AddSessionToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Timetables.Identifiers;

public sealed record AddSessionToOfferingCommand(
    OfferingId OfferingId,
    PeriodId PeriodId)
    : ICommand;
