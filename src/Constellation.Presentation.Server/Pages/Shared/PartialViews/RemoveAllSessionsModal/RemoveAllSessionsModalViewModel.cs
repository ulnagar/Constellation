namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveAllSessionsModal;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

internal sealed record RemoveAllSessionsModalViewModel(
    OfferingId OfferingId,
    string CourseName,
    string OfferingName);
