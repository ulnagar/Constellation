namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Reports;

using Application.Models.Auth;
using Application.Training.Modules.GetModuleStatusByStaffMember;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class StaffMemberModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public StaffMemberModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; } = new();

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Reports;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<List<ModuleStatusResponse>> request = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(Id));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Reports/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Modules = request.Value;
    }
}