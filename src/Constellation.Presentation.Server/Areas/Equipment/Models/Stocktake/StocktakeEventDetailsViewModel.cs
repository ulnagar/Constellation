using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake
{
    public class StocktakeEventDetailsViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalResponses { get; set; }
        public int TotalDevices { get; set; }
        public int RemainingDevices { get; set; }
        public ICollection<Sighting> Sightings { get; set; } = new List<Sighting>();

        public class Sighting
        {
            public Guid Id { get; set; }
            public string AssetNumber { get; set; }
            public string SerialNumber { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string User { get; set; }
            public string SightedBy { get; set; }
            public DateTime SightedOn { get; set; }
        }
    }
}
