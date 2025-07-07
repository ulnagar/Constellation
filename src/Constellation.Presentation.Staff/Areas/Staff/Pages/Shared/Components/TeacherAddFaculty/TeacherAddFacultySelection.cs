namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TeacherAddFaculty;

using Core.Models.StaffMembers.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultySelection
{
    public StaffId StaffId { get; set; }
    public string StaffName { get; set; }

    public Guid FacultyId { get; set; }

    public string Role { get; set; }

    public SelectList FacultyRoles { get; set; }
    public SelectList Faculties { get; set; }
    public string ReturnUrl { get; set; }
}