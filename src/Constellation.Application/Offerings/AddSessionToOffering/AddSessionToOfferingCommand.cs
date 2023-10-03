namespace Constellation.Application.Offerings.AddSessionToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record AddSessionToOfferingCommand(
    OfferingId OfferingId,
    int PeriodId)
    : ICommand;
