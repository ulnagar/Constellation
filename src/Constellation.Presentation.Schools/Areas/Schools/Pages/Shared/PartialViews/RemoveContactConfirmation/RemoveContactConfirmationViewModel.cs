namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.RemoveContactConfirmation;

using Application.Domains.SchoolContacts.Queries.GetContactsWithRoleFromSchool;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;
using System.ComponentModel.DataAnnotations;

public sealed class RemoveContactConfirmationViewModel
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContactRoleId AssignmentId { get; set; }
    public ContactResponse Contact { get; set; }
    public EmailAddress ContactEmail { get; set; }
    [Required]
    public string Comment { get; set; }
}
