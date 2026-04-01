namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddPhoneNumberToSchoolContact;

using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed class AddPhoneNumberToSchoolContactViewModel
{
    private AddPhoneNumberToSchoolContactViewModel() { }

    public AddPhoneNumberToSchoolContactViewModel(
        PhoneNumber phoneNumber,
        SchoolContactId contactId)
    {
        PhoneNumber = phoneNumber.ToString(Core.ValueObjects.PhoneNumber.Format.None);
        ContactId = contactId;
    }

    public string PhoneNumber { get; private set; }
    public SchoolContactId ContactId { get; private set; }
}
