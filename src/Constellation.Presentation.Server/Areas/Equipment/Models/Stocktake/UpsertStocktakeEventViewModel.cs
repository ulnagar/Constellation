using Constellation.Application.Features.Equipment.Stocktake.Commands;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake
{
    public class UpsertStocktakeEventViewModel : BaseViewModel
    {
        public UpsertStocktakeEventCommand Command { get; set; } = new UpsertStocktakeEventCommand();
    }
}
