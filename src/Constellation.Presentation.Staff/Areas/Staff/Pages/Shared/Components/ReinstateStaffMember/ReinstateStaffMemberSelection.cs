namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStaffMember;

using Constellation.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class ReinstateStaffMemberSelection
{
    public string SchoolCode { get; set; } = string.Empty;

    public required SelectList SchoolList { get; set; }

}