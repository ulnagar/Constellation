namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Application.Models.Auth;
using Constellation.Application.Students.GetStudentsWithAbsenceSettings;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class AuditModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AuditModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    public List<StudentAbsenceSettingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        ViewData["ActivePage"] = "Audit";

        await GetClasses(_mediator);

        var studentRequest = await _mediator.Send(new GetStudentsWithAbsenceSettingsQuery());

        if (studentRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentRequest.Error,
                RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
            };

            return;
        }
        
        Students = studentRequest
            .Value
            .OrderBy(student => student.School)
            .ThenBy(student => student.Grade)
            .ThenBy(student => student.Name)
            .ToList();

        return;
    }
}
