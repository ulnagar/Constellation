namespace Constellation.Application.Offerings.RemoveResourceFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record RemoveResourceFromOfferingCommand(
    OfferingId OfferingId,
    ResourceId ResourceId)
    : ICommand;