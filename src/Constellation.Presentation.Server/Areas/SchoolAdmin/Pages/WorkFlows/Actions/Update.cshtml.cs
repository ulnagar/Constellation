namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Actions;

using Application.Models.Auth;
using Application.WorkFlows.UpdateConfirmSentralEntryAction;
using Application.WorkFlows.UpdateCreateSentralEntryAction;
using BaseModels;
using Constellation.Application.WorkFlows.GetCaseById;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Pages.Shared.Components.ActionUpdateForm;
using Workflows;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpdateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => WorkFlowPages.Cases;

    [BindProperty(SupportsGet = true)]
    public Guid CaseGuid { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ActionGuid { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(CaseGuid)));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/WorkFlows/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Case = request.Value;
    }

    public async Task<IActionResult> OnPostUpdateSentralEntryAction(CreateSentralEntryActionViewModel viewModel)
    {
        Result attempt = await _mediator.Send(new UpdateCreateSentralEntryActionCommand(CaseId.FromValue(CaseGuid), ActionId.FromValue(ActionGuid), viewModel.NotRequired, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }
        
        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseGuid });
    }

    public async Task<IActionResult> OnPostUpdateConfirmEntryAction(ConfirmSentralEntryActionViewModel viewModel)
    {
        Result attempt = await _mediator.Send(new UpdateConfirmSentralEntryActionCommand(CaseId.FromValue(CaseGuid), ActionId.FromValue(ActionGuid), viewModel.Confirmed));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseGuid });
    }
}