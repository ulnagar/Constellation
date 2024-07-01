namespace Constellation.Presentation.Staff.Views.Shared.PartialViews.UpdateRoleNoteModal;

using Constellation.Core.Models.SchoolContacts.Identifiers;

public class UpdateRoleNoteModalViewModel
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContactRoleId RoleId { get; set; }
    public string Note { get; set; }
}
