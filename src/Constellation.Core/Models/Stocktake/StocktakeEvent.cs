namespace Constellation.Core.Models.Stocktake;

using System;
using System.Collections.Generic;

public sealed class StocktakeEvent
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } 
    public bool AcceptLateResponses { get; set; }
    public string Name { get; set; }
    public ICollection<StocktakeSighting> Sightings { get; set; } = new List<StocktakeSighting>();
}