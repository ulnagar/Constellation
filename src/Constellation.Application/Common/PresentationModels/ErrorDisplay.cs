namespace Constellation.Application.Common.PresentationModels;

using Constellation.Core.Shared;

public class ErrorDisplay
{
    public Error Error { get; set; }
    public string? RedirectPath { get; set; }
}
