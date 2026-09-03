namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmRemoveStaffFromFacultyModal;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed class ConfirmRemoveStaffFromFacultyModalViewModel
{
    public required string StaffName { get; set; }
    public required StaffId StaffId { get; set; }
}
