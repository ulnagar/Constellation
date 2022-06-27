using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake
{
    public class ListStocktakeEventsViewModel : BaseViewModel
    {
        public ICollection<StocktakeEventItem> Stocktakes { get; set; } = new List<StocktakeEventItem>();

        public class StocktakeEventItem
        {
            public Guid Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool AcceptLateResponses { get; set; }
            public string Name { get; set; }
        }
    }
}
