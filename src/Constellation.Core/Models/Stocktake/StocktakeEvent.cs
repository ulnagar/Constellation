namespace Constellation.Core.Models.Stocktake;

public class StocktakeEvent
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } 
    public bool AcceptLateResponses { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<StocktakeSighting> Sightings { get; set; } = new();
}
