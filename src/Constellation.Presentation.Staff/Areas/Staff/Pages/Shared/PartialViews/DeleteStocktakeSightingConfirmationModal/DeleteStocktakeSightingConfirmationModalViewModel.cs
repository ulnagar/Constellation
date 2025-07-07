namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.DeleteStocktakeSightingConfirmationModal;

using Core.Models.Stocktake.Identifiers;

public sealed record DeleteStocktakeSightingConfirmationModalViewModel(
    StocktakeEventId EventId,
    StocktakeSightingId SightingId)
{
    public string Comment { get; set; } = string.Empty;
}