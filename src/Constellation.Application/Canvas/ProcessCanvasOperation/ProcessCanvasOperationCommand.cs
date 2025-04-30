namespace Constellation.Application.Canvas.ProcessCanvasOperation;

using Abstractions.Messaging;

public sealed record ProcessCanvasOperationCommand(
    int OperationId,
    bool UseSections)
    : ICommand;