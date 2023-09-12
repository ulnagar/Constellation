namespace Constellation.Application.Offerings.AddSessionToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record AddSessionToOfferingCommand(
    OfferingId OfferingId,
    int PeriodId)
    : ICommand;
