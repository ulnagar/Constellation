namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.UpdateRoleNoteModal;

using Core.Models.SchoolContacts.Identifiers;

public class UpdateRoleNoteModalViewModel
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContactRoleId RoleId { get; set; }
    public string Note { get; set; }
}
