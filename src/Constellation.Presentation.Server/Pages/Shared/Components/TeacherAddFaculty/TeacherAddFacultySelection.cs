namespace Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;

using Constellation.Presentation.Server.BaseModels;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.ValueObjects;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultySelection : BaseViewModel
{
    public string StaffId { get; set; }
    public string StaffName { get; set; }

    public FacultyId FacultyId { get; set; }

    public FacultyMembershipRole Role { get; set; }

    public SelectList FacultyRoles { get; set; }
    public SelectList Faculties { get; set; }
    public string ReturnUrl { get; set; }
}