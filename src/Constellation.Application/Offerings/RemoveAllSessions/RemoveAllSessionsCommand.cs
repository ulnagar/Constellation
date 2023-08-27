namespace Constellation.Application.Offerings.RemoveAllSessions;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record RemoveAllSessionsCommand(
    OfferingId OfferingId)
    : ICommand;