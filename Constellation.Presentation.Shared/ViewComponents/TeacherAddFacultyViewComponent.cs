namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Faculties.GetFacultiesAsDictionary;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty.Identifiers;
using Constellation.Core.Models.Faculty.ValueObjects;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.TeacherAddFaculty;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TeacherAddFacultyViewComponent : ViewComponent
{
    private readonly IMediator _mediator;
    private readonly IStaffRepository _staffRepository;

    public TeacherAddFacultyViewComponent(
        IMediator mediator,
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
        {
            viewModel.Faculties = new SelectList(facultyList.Value, "Key", "Value");
        }
        else
        {
            viewModel.Faculties = new SelectList(null, "");
        }

        Staff staffMember = await _staffRepository.FromIdForExistCheck(staffId);

        if (staffMember is null)
        {
            return View(viewModel);
        }

        viewModel.StaffId = staffId;
        viewModel.StaffName = staffMember.DisplayName;
        viewModel.FacultyRoles = new SelectList(FacultyMembershipRole.Enumerations(), "");

        return View(viewModel);
    }
}
