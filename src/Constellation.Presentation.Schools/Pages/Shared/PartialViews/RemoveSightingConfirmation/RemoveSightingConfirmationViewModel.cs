namespace Constellation.Presentation.Schools.Pages.Shared.PartialViews.RemoveSightingConfirmation;

using Core.Models.Stocktake.Identifiers;
using System;

public sealed class RemoveSightingConfirmationViewModel
{
    public StocktakeEventId EventId { get; set; }
    public StocktakeSightingId SightingId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string AssetNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string SightedBy { get; set; } = string.Empty;
    public DateOnly SightedAt { get; set; }
    public string Comment { get; set; } = string.Empty;
}
