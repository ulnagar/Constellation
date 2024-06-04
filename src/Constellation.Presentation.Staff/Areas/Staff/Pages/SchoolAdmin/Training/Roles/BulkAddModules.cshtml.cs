namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Roles;

using Application.Models.Auth;
using Application.Training.Models;
using Application.Training.Modules.GetListOfModuleSummary;
using Application.Training.Roles.AddModuleToTrainingRole;
using Application.Training.Roles.GetTrainingRoleDetails;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class BulkAddModulesModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public BulkAddModulesModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TrainingRoleDetailResponse Role { get; set; }
    public List<ModuleSummaryDto> Modules { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedModuleIds { get; set; } = new();

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Roles;

    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
    TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result<TrainingRoleDetailResponse> roleRequest = await _mediator.Send(new GetTrainingRoleDetailsQuery(roleId));

        if (roleRequest.IsFailure)
        {
            Error = new()
            {
                Error = roleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Details", values: new { area = "Staff", Id })
            };

            return;
        }

        Role = roleRequest.Value;

        Result<List<ModuleSummaryDto>> modulesRequest = await _mediator.Send(new GetListOfModuleSummaryQuery());

        if (modulesRequest.IsFailure)
        {
            Error = new()
            {
                Error = modulesRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Details", values: new { area = "Staff", Id })
            };

            return;
        }

        Modules = modulesRequest.Value.Where(member => member.IsActive).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        if (SelectedModuleIds.Count == 0)
        {
            ModelState.AddModelError("", "You must select at least one Module to add");

            await PreparePage();

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new AddModuleToTrainingRoleCommand(roleId, SelectedModuleIds.Select(TrainingModuleId.FromValue).ToList()));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", Id });
    }

}