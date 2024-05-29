namespace Constellation.Presentation.Server.BaseModels;

using Application.Common.PresentationModels;

public class BaseViewModel : IBaseModel
{
    // Application Settings
    public ErrorDisplay Error { get; set; }
}