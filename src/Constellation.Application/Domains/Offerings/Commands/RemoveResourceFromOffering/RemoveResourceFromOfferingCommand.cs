namespace Constellation.Application.Domains.Offerings.Commands.RemoveResourceFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveResourceFromOfferingCommand(
    OfferingId OfferingId,
    ResourceId ResourceId)
    : ICommand;