namespace Constellation.Application.Domains.Tutorials.Tutorials.Commands.RemoveAllSessionsFromTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record RemoveAllSessionsFromTutorialCommand(
    TutorialId TutorialId)
    : ICommand;