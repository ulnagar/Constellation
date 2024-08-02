namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AssignRoleModal;

using Core.Models.SchoolContacts.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AssignRoleModalViewModel
{
    public string SchoolCode { get; set; }
    public string RoleName { get; set; }
    public SchoolContactId ContactId { get; set; }
    public string Note { get; set; }

    public SelectList Roles { get; set; }
    public SelectList Schools { get; set; }
    public string ContactName { get; set; }
}
