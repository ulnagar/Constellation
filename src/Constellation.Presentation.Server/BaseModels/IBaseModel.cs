namespace Constellation.Presentation.Server.BaseModels;

using Constellation.Core.Models.Subjects.Identifiers;

public interface IBaseModel
{
    public IDictionary<string, OfferingId> Classes { get; set; }
    public ErrorDisplay Error { get; set; }
}
