namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveSessionModal;

using Constellation.Core.Models.Offerings.Identifiers;

internal sealed record RemoveSessionModalViewModel(
    OfferingId OfferingId,
    SessionId SessionId,
    string SessionName,
    string CourseName,
    string OfferingName);
