namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddPhoneNumberToSchoolContact;

using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;

public sealed class AddPhoneNumberToSchoolContactViewModel
{
    [ModelBinder(typeof(FromValueBinder))]
    public PhoneNumber PhoneNumber { get; set; }
    public SchoolContactId ContactId { get; set; }
}
