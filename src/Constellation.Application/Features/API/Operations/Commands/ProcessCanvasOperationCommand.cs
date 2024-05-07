namespace Constellation.Application.Features.API.Operations.Commands;

using Abstractions.Messaging;

public sealed record ProcessCanvasOperationCommand(
    int OperationId)
    : ICommand;