namespace Constellation.Application.Domains.Tutorials.Requests.Commands.ApproveTutorialRequest;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record ApproveTutorialRequestCommand(
    RequestId RequestId,
    string Comment)
    : ICommand;
