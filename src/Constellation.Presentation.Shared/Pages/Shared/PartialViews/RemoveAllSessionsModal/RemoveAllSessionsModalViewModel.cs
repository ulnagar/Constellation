namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.RemoveAllSessionsModal;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveAllSessionsModalViewModel(
    OfferingId OfferingId,
    string CourseName,
    string OfferingName);
