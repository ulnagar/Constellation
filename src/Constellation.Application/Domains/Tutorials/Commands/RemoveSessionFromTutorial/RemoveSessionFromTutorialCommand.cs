namespace Constellation.Application.Domains.Tutorials.Commands.RemoveSessionFromTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record RemoveSessionFromTutorialCommand(
    TutorialId TutorialId,
    TutorialSessionId SessionId)
    : ICommand;