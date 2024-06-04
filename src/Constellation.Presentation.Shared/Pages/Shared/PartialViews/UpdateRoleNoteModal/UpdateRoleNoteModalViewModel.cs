namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.UpdateRoleNoteModal;

using Constellation.Core.Models.SchoolContacts.Identifiers;

public class UpdateRoleNoteModalViewModel
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContactRoleId RoleId { get; set; }
    public string Note { get; set; }
}
