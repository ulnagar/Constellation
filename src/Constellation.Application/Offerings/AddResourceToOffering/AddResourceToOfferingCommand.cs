namespace Constellation.Application.Offerings.AddResourceToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record AddResourceToOfferingCommand(
    OfferingId OfferingId,
    ResourceType ResourceType,
    string Name,
    string Url,
    string ResourceId)
    : ICommand;
