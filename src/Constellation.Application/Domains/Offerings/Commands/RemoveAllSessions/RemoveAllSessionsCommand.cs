namespace Constellation.Application.Domains.Offerings.Commands.RemoveAllSessions;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveAllSessionsCommand(
    OfferingId OfferingId)
    : ICommand;