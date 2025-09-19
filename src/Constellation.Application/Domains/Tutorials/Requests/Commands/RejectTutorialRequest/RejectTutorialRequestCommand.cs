namespace Constellation.Application.Domains.Tutorials.Requests.Commands.RejectTutorialRequest;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record RejectTutorialRequestCommand(
    RequestId RequestId,
    string Comment)
    : ICommand;