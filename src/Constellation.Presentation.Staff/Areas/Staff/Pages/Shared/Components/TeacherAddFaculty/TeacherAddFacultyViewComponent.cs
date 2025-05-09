﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TeacherAddFaculty;

using Application.Domains.Faculties.Queries.GetFacultiesAsDictionary;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Faculties.ValueObjects;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultyViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    private readonly IStaffRepository _staffRepository;

    public TeacherAddFacultyViewComponent(
        ISender mediator,
        IStaffRepository staffRepository)
    {
        _mediator = mediator;
        _staffRepository = staffRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(string staffId)
    {
        TeacherAddFacultySelection viewModel = new();

        Result<Dictionary<FacultyId, string>> facultyList = await _mediator.Send(new GetFacultiesAsDictionaryQuery());

        if (facultyList.IsSuccess)
            viewModel.Faculties = new SelectList(facultyList.Value, "Key", "Value");
        else
        {
            viewModel.Faculties = new SelectList(null, "");
        }

        Staff staffMember = await _staffRepository.FromIdForExistCheck(staffId);

        if (staffMember is null)
            return View(viewModel);

        viewModel.StaffId = staffId;
        viewModel.StaffName = staffMember.DisplayName;
        viewModel.FacultyRoles = new SelectList(FacultyMembershipRole.Enumerations(), "");

        return View(viewModel);
    }
}
