namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStaffMember;

using Core.Models.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class ReinstateStaffMemberSelection
{
    public SchoolCode SchoolCode { get; set; } = SchoolCode.Empty;

    public required SelectList SchoolList { get; set; }

}