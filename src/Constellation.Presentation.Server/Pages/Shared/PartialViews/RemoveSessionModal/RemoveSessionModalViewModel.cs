namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveSessionModal;

using Constellation.Core.Models.Subjects.Identifiers;

internal sealed record RemoveSessionModalViewModel(
    OfferingId OfferingId,
    int SessionId,
    string SessionName,
    string TeacherName,
    string CourseName,
    string OfferingName);
