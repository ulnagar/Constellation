namespace Constellation.Core.Models.Assets.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;
using ValueObjects;

public sealed record AssetRegisteredDomainEvent(
    DomainEventId Id,
    AssetId AssetId,
    AssetNumber AssetNumber)
    : DomainEvent(Id);