using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Stocktake;
using System;

namespace Constellation.Application.Features.Portal.School.Stocktake.Models
{
    public class StocktakeSightingsForList : IMapFrom<StocktakeSighting>
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
