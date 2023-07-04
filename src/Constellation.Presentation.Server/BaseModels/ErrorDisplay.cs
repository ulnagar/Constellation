namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Core.Shared;

public class ErrorDisplay
{
    public Error Error { get; set; }
    public string? RedirectPath { get; set; }
}
