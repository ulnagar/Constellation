namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.WorkFlows.AddActionNote;
using Application.WorkFlows.AddCaseDetailUpdateAction;
using Application.WorkFlows.AddParentInterviewAction;
using Application.WorkFlows.AddPhoneParentAction;
using Application.WorkFlows.AddSendEmailAction;
using Application.WorkFlows.AddSentralEntryAction;
using Application.WorkFlows.CancelAction;
using Application.WorkFlows.GetCaseById;
using Application.WorkFlows.ReassignAction;
using Application.WorkFlows.UpdateCaseStatus;
using Constellation.Application.Features.Common.Queries;
using Core.Models.Offerings.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using Presentation.Shared.Pages.Shared.PartialViews.AddActionNoteModal;
using Presentation.Shared.Pages.Shared.PartialViews.AddCaseDetailUpdateAction;
using Presentation.Shared.Pages.Shared.PartialViews.AddCreateSentralEntryAction;
using Presentation.Shared.Pages.Shared.PartialViews.AddParentInterviewAction;
using Presentation.Shared.Pages.Shared.PartialViews.AddPhoneParentAction;
using Presentation.Shared.Pages.Shared.PartialViews.AddSendEmailAction;
using Presentation.Shared.Pages.Shared.PartialViews.ConfirmActionUpdateModal;
using Presentation.Shared.Pages.Shared.PartialViews.ReassignActionToStaffMemberModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public CaseId Id { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Index", values: new { area = "Staff" })
            };

            return;
        }

        Case = request.Value;
    }

    public async Task<IActionResult> OnPostUpdateStatus(string status)
    {
        CaseStatus newStatus = CaseStatus.FromValue(status);

        Result action = await _mediator.Send(new UpdateCaseStatusCommand(Id, newStatus));

        if (action.IsFailure)
        {
            Error = new()
            {
                Error = action.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxActionConfirmation(Guid actionId)
    {
        ConfirmActionUpdateModalViewModel viewModel = new(actionId);

        return Partial("ConfirmActionUpdateModal", viewModel);
    }

    public async Task<IActionResult> OnGetCancelAction(Guid actionId)
    {
        Result cancelRequest = await _mediator.Send(new CancelActionCommand(Id, ActionId.FromValue(actionId)));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxAddActionNote(Guid actionId)
    {
        AddActionNoteModalViewModel viewModel = new()
        {
            ActionId = ActionId.FromValue(actionId)
        };

        return Partial("AddActionNoteModal", viewModel);
    }

    public async Task<IActionResult> OnPostAddActionNote(Guid actionId, string note)
    {
        Result noteRequest = await _mediator.Send(new AddActionNoteCommand(Id, ActionId.FromValue(actionId), note));

        if (noteRequest.IsFailure)
        {
            Error = new()
            {
                Error = noteRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxReassignAction(Guid actionId)
    {
        Dictionary<string, string> staffResult = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        ReassignActionToStaffMemberModalViewModel viewModel = new()
        {
            ActionId = ActionId.FromValue(actionId),
            StaffMembers = staffResult
        };

        return Partial("ReassignActionToStaffMemberModal", viewModel);
    }

    public async Task<IActionResult> OnPostReassignAction(Guid actionId, string staffId)
    {
        Result reassignRequest = await _mediator.Send(new ReassignActionCommand(Id, ActionId.FromValue(actionId), staffId));

        if (reassignRequest.IsFailure)
        {
            Error = new()
            {
                Error = reassignRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxNewAction(string actionType)
    {
        Dictionary<string, string> staffResult = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        switch (actionType)
        {
            case nameof(SendEmailAction):
                AddSendEmailActionViewModel emailViewModel = new()
                {
                    StaffMembers = staffResult
                };

                return Partial("AddSendEmailAction", emailViewModel);
                break;
            case nameof(CreateSentralEntryAction):
                Result<List<OfferingSelectionListResponse>> offerings = await _mediator.Send(new GetOfferingsForSelectionListQuery());

                if (offerings.IsFailure) return BadRequest();

                AddCreateSentralEntryActionViewModel sentralViewModel = new()
                {
                    StaffMembers = staffResult,
                    Offerings = offerings.Value.ToDictionary(k => k.Id.Value, k => k.Name)
                };

                return Partial("AddCreateSentralEntryAction", sentralViewModel);
                break;
            case nameof(PhoneParentAction):
                AddPhoneParentActionViewModel phoneViewModel = new()
                {
                    StaffMembers = staffResult
                };

                return Partial("AddPhoneParentAction", phoneViewModel);
                break;
            case nameof(ParentInterviewAction):
                AddParentInterviewActionViewModel interviewViewModel = new()
                {
                    StaffMembers = staffResult
                };

                return Partial("AddParentInterviewAction", interviewViewModel);
                break;
            case nameof(CaseDetailUpdateAction):
                AddCaseDetailUpdateActionViewModel detailViewModel = new();

                return Partial("AddCaseDetailUpdateAction", detailViewModel);

                break;
            default:
                return BadRequest();
        }
    }

    public async Task<IActionResult> OnPostNewSentralAction(AddCreateSentralEntryActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = CaseErrors.Action.Assign.StaffNull,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSentralEntryActionCommand(Id, OfferingId.FromValue(viewModel.OfferingId), viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewDetailAction(AddCaseDetailUpdateActionViewModel viewModel)
    {
        string staffMemberId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        Result actionRequest = await _mediator.Send(new AddCaseDetailUpdateActionCommand(Id, staffMemberId, viewModel.Details));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewInterviewAction(AddParentInterviewActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = CaseErrors.Action.Assign.StaffNull,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddParentInterviewActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewPhoneAction(AddPhoneParentActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = CaseErrors.Action.Assign.StaffNull,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddPhoneParentActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewEmailAction(AddSendEmailActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = CaseErrors.Action.Assign.StaffNull,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSendEmailActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }
}