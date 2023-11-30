namespace Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;

using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultySelection : BaseViewModel
{
    public string StaffId { get; set; }
    public string StaffName { get; set; }

    public Guid FacultyId { get; set; }

    public string Role { get; set; }

    public SelectList FacultyRoles { get; set; }
    public SelectList Faculties { get; set; }
    public string ReturnUrl { get; set; }
}