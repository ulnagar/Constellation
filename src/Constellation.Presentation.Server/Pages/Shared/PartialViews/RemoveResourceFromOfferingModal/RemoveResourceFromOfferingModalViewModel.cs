namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveResourceFromOfferingModal;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

internal sealed record RemoveResourceFromOfferingModalViewModel(
    OfferingId OfferingId,
    ResourceId ResourceId,
    string ResourceName,
    string ResourceType,
    string CourseName,
    string OfferingName);