namespace Constellation.Presentation.Schools.Pages.Shared.PartialViews.RemoveContactConfirmation;

using Application.SchoolContacts.GetContactsWithRoleFromSchool;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;
using System.ComponentModel.DataAnnotations;

public sealed class RemoveContactConfirmationViewModel
{
    [ModelBinder(typeof(ConstructorBinder))]
    public SchoolContactId ContactId { get; set; }
    [ModelBinder(typeof(ConstructorBinder))]
    public SchoolContactRoleId AssignmentId { get; set; }
    public ContactResponse Contact { get; set; }
    [Required]
    public string Comment { get; set; }
}
