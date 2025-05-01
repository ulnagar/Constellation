namespace Constellation.Application.Domains.LinkedSystems.Canvas.Errors;

using Core.Shared;

public static class CanvasErrors
{
    public static class Rubric
    {
        public static Error NotFound = new(
            "Canvas.Rubric.NotFound",
            $"The selected Assignment does not have an attached Rubric");
    }

}
