namespace Constellation.Application.Domains.Offerings.Commands.AddResourceToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Offerings.Enums;

public sealed record AddResourceToOfferingCommand(
    OfferingId OfferingId,
    ResourceType ResourceType,
    string Name,
    string Url,
    string ResourceId)
    : ICommand;
