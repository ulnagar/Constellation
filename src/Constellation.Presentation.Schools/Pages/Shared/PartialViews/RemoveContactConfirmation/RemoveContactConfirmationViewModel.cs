namespace Constellation.Presentation.Schools.Pages.Shared.PartialViews.RemoveContactConfirmation;

using Application.SchoolContacts.GetContactsWithRoleFromSchool;
using Core.Models.SchoolContacts.Identifiers;

public sealed class RemoveContactConfirmationViewModel
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContactRoleId AssignmentId { get; set; }
    public ContactResponse Contact { get; set; }
    public string Comment { get; set; }
}
