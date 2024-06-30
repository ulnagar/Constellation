namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Models.Auth;
using Application.Students.GetCurrentStudentsWithoutSentralId;
using Application.Students.Models;
using Application.Students.UpdateStudentSentralId;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class WithoutSentralIdModel : BasePageModel
{
    private readonly ISender _mediator;

    public WithoutSentralIdModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;

    public List<StudentResponse> Students { get; set; }

    public async Task OnGet()
    {
        Result<List<StudentResponse>> request = await _mediator.Send(new GetCurrentStudentsWithoutSentralIdQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Students = request.Value;
    }

    public async Task<IActionResult> OnGetUpdateSentralIds()
    {
        Result<List<StudentResponse>> request = await _mediator.Send(new GetCurrentStudentsWithoutSentralIdQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        Students = request.Value;

        foreach (StudentResponse student in Students)
        {
            await _mediator.Send(new UpdateStudentSentralIdCommand(student.StudentId));
        }

        return RedirectToPage();
    }
}