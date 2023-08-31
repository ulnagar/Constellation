namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveSessionModal;

using Constellation.Core.Models.Offerings.Identifiers;

internal sealed record RemoveSessionModalViewModel(
    OfferingId OfferingId,
    int SessionId,
    string SessionName,
    string TeacherName,
    string CourseName,
    string OfferingName);
