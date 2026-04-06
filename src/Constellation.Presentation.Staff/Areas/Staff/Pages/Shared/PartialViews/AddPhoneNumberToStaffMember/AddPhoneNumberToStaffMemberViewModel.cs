namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddPhoneNumberToStaffMember;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed class AddPhoneNumberToStaffMemberViewModel
{
    private AddPhoneNumberToStaffMemberViewModel() { }

    public AddPhoneNumberToStaffMemberViewModel(
        PhoneNumber phoneNumber,
        StaffId staffId)
    {
        PhoneNumber = phoneNumber.ToString(Core.ValueObjects.PhoneNumber.Format.None);
        StaffId = staffId;
    }

    public string PhoneNumber { get; private set; }
    public StaffId StaffId { get; set; }
}
