namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Application.Models.Auth;
using Constellation.Application.Students.GetStudentsWithAbsenceSettings;
using Constellation.Core.Models.Absences;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.All;

    public List<StudentAbsenceSettingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        ViewData["ActivePage"] = "Audit";

        await GetClasses(_mediator);

        Result<List<StudentAbsenceSettingsResponse>> studentRequest = await _mediator.Send(new GetStudentsWithAbsenceSettingsQuery());

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

        if (Filter == FilterDto.Disabled)
        {
            Students = Students
                .Where(student =>
                {
                    bool anyWholeAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Whole);
                    bool anyPartialAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Partial);

                    return (!anyWholeAbsences || !anyPartialAbsences);
                })
                .ToList();
        }

        if (Filter == FilterDto.Enabled)
        {
            Students = Students
                .Where(student =>
                {
                    bool anyWholeAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Whole);
                    bool anyPartialAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Partial);

                    return (anyWholeAbsences || anyPartialAbsences);
                })
                .ToList();
        }
    }

    public enum FilterDto
    {
        All,
        Enabled,
        Disabled
    }
}
