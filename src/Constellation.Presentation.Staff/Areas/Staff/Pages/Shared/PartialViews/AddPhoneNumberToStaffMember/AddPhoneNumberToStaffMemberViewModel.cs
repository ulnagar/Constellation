namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddPhoneNumberToStaffMember;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;

public sealed class AddPhoneNumberToStaffMemberViewModel
{
    [ModelBinder(typeof(FromValueBinder))]
    public PhoneNumber PhoneNumber { get; set; }
    public StaffId StaffId { get; set; }
}
