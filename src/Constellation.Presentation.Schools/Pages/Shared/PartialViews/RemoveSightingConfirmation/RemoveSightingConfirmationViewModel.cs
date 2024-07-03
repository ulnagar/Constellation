namespace Constellation.Presentation.Schools.Pages.Shared.PartialViews.RemoveSightingConfirmation;
using System;

public sealed class RemoveSightingConfirmationViewModel
{
    public Guid EventId { get; set; }
    public Guid SightingId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string AssetNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string SightedBy { get; set; } = string.Empty;
    public DateOnly SightedAt { get; set; }
    public string Comment { get; set; } = string.Empty;
}
