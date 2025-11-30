namespace Constellation.Application.Domains.Tutorials.Requests.Commands.AddNoteToTutorialRequest;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record AddNoteToTutorialRequestCommand(
    RequestId RequestId,
    string Message)
    : ICommand;