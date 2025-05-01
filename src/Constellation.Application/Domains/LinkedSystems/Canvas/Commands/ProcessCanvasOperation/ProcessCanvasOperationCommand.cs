namespace Constellation.Application.Domains.LinkedSystems.Canvas.Commands.ProcessCanvasOperation;

using Abstractions.Messaging;

public sealed record ProcessCanvasOperationCommand(
    int OperationId,
    bool UseSections)
    : ICommand;