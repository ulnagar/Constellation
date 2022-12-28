namespace Constellation.Presentation.Server.BaseModels;

public interface IBaseModel
{
    public IDictionary<string, int> Classes { get; set; }
    public ErrorDisplay Error { get; set; }
}
