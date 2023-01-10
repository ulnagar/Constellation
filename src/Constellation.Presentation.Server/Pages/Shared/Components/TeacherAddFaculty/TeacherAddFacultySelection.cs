﻿namespace Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;

using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultySelection : BaseViewModel
{
    public string StaffId { get; set; }
    public string StaffName { get; set; }

    public Guid FacultyId { get; set; }

    public FacultyMembershipRole Role { get; set; }

    public SelectList Faculties { get; set; }
    public string ReturnUrl { get; set; }
}