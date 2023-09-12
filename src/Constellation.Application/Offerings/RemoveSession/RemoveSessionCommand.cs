namespace Constellation.Application.Offerings.RemoveSession;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record RemoveSessionCommand(
    OfferingId OfferingId,
    SessionId SessionId)
    : ICommand;
