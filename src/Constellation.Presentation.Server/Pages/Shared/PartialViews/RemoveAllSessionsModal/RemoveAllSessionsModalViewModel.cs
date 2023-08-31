namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveAllSessionsModal;

using Constellation.Core.Models.Offerings.Identifiers;

internal sealed record RemoveAllSessionsModalViewModel(
    OfferingId OfferingId,
    string CourseName,
    string OfferingName);
