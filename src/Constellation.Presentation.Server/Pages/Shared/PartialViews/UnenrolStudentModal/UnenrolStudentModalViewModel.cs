namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.UnenrolStudentModal;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

internal sealed record UnenrolStudentModalViewModel(
    OfferingId OfferingId,
    string StudentId,
    string StudentName,
    string CourseName,
    string OfferingName);
