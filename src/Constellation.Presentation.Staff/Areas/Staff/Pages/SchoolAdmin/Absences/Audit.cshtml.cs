namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetStudentsWithAbsenceSettings;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Audit;
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.All;

    public List<StudentAbsenceSettingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<StudentAbsenceSettingsResponse>> studentRequest = await _mediator.Send(new GetStudentsWithAbsenceSettingsQuery());

        if (studentRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                studentRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

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
