namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.RemoveSessionModal;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveSessionModalViewModel(
    OfferingId OfferingId,
    SessionId SessionId,
    string SessionName,
    string CourseName,
    string OfferingName);
