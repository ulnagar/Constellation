namespace Constellation.Presentation.Shared.Pages.Shared.Components.StaffNavBar;

using Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;

public sealed class StaffNavBarViewModel
{
    public List<TeacherOfferingResponse> Classes { get; set; } = new();

    public bool CanAccessParentPortal { get; set; }
    public bool CanAccessSchoolPortal { get; set; }
}