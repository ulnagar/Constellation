namespace Constellation.Application.Offerings.RemoveSession;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record RemoveSessionCommand(
    int SessionId,
    OfferingId OfferingId)
    : ICommand;
