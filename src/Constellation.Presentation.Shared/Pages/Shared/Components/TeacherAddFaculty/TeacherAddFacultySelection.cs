namespace Constellation.Presentation.Shared.Pages.Shared.Components.TeacherAddFaculty;

using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultySelection
{
    public string StaffId { get; set; }
    public string StaffName { get; set; }

    public Guid FacultyId { get; set; }

    public string Role { get; set; }

    public SelectList FacultyRoles { get; set; }
    public SelectList Faculties { get; set; }
    public string ReturnUrl { get; set; }
}