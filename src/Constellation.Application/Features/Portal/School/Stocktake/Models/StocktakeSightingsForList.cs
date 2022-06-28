using System;

namespace Constellation.Application.Features.Portal.School.Stocktake.Models
{
    public class StocktakeSightingsForList
    {
        public string SerialNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string LocationName { get; set; }
        public string UserName { get; set; }
        public string SightedBy { get; set; }
        public DateTime SightedAt { get; set; }
    }
}
