namespace Constellation.Presentation.Server.Areas.Partner.Pages.Students;

using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetStudentsWithAbsenceSettings;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class AbsenceAuditModel : BasePageModel
{
    private readonly IMediator _mediator;

    public AbsenceAuditModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<StudentAbsenceSettingsResponse> Students { get; set; }

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        var studentRequest = await _mediator.Send(new GetStudentsWithAbsenceSettingsQuery());

        if (studentRequest.IsFailure)
        {
            return RedirectToAction("Index", "Students", new { area = "Partner" });
        }

        Students = studentRequest.Value;

        return Page();
    }
}
