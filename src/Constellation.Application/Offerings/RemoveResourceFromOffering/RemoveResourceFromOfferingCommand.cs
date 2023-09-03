namespace Constellation.Application.Offerings.RemoveResourceFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveResourceFromOfferingCommand(
    OfferingId OfferingId,
    ResourceId ResourceId)
    : ICommand;