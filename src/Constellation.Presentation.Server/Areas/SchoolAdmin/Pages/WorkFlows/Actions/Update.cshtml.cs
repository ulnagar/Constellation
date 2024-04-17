namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Actions;

using Application.Models.Auth;
using Application.WorkFlows.UpdateConfirmSentralEntryAction;
using Application.WorkFlows.UpdateCreateSentralEntryAction;
using Application.WorkFlows.UpdateParentInterviewAction;
using Application.WorkFlows.UpdatePhoneParentAction;
using BaseModels;
using Constellation.Application.WorkFlows.GetCaseById;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using Helpers.ModelBinders;
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

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public CaseId CaseId { get; set; }

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public ActionId ActionId { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId));

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
        Result attempt = await _mediator.Send(new UpdateCreateSentralEntryActionCommand(CaseId, ActionId, viewModel.NotRequired, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }
        
        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateConfirmEntryAction(ConfirmSentralEntryActionViewModel viewModel)
    {
        Result attempt = await _mediator.Send(new UpdateConfirmSentralEntryActionCommand(CaseId, ActionId, viewModel.Confirmed));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdatePhoneParentAction(PhoneParentActionViewModel viewModel)
    {
        Result attempt = await _mediator.Send(new UpdatePhoneParentActionCommand(CaseId, ActionId, viewModel.ParentName, viewModel.ParentPhoneNumber, viewModel.DateOccurred, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateParentInterviewAction(ParentInterviewActionViewModel viewModel)
    {
        List<InterviewAttendee> attendees = new();

        foreach (ParentInterviewActionViewModel.Attendee attendee in viewModel.Attendees)
            attendees.Add(InterviewAttendee.Create(ActionId, attendee.Name, attendee.Notes));

        Result attempt = await _mediator.Send(new UpdateParentInterviewActionCommand(CaseId, ActionId, attendees, viewModel.DateOccurred, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseId.Value });
    }
}