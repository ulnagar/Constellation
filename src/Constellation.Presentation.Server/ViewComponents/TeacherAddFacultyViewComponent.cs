namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;
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
        var viewModel = new TeacherAddFacultySelection();

        var facultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

        var staffMember = await _staffRepository.FromIdForExistCheck(staffId);

        if (staffMember is null)
        {
            return View(viewModel);
        }

        viewModel.StaffId = staffId;
        viewModel.StaffName = staffMember.DisplayName;
        viewModel.Faculties = new SelectList(facultyList, "Key", "Value");

        return View(viewModel);
    }
}
