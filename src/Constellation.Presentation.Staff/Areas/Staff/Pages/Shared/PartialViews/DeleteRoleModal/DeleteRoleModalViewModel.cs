namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.DeleteRoleModal;

using Core.Models.SchoolContacts.Identifiers;

public sealed record DeleteRoleModalViewModel(
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId,
    string ContactName,
    string ContactRole,
    string SchoolName);
