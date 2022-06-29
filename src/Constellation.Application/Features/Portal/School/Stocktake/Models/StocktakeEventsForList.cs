using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Stocktake;
using System;

namespace Constellation.Application.Features.Portal.School.Stocktake.Models
{
    public class StocktakeEventsForList : IMapFrom<StocktakeEvent>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
