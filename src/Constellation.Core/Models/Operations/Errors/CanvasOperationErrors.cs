namespace Constellation.Core.Models.Operations.Errors;

using Shared;
using System;

public static class CanvasOperationErrors
{
    public static readonly Error Invalid = new(
        "Operations.Canvas.Invalid",
        "Failed to find valid code path for Canvas Operation");

    public static readonly Func<int, Error> NotFound = id => new(
        "Operations.Canvas.NotFound",
        $"Could not find a Canvas Operation with the Id {id}");

    public static readonly Error ProcessFailed = new(
        "Operations.Canvas.ProcessFailed",
        "Failed to process the Canvas Operation");
}