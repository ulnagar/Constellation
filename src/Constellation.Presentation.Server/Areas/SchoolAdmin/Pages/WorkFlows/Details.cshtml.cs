namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Workflows;

using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.StaffMembers.GetStaffById;
using Application.WorkFlows.AddActionNote;
using Application.WorkFlows.AddCaseDetailUpdateAction;
using Application.WorkFlows.AddSentralEntryAction;
using Application.WorkFlows.CancelAction;
using Application.WorkFlows.CancelCase;
using Application.WorkFlows.CloseCase;
using Application.WorkFlows.GetCaseById;
using Application.WorkFlows.ReassignAction;
using BaseModels;
using Constellation.Application.Features.Common.Queries;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Offerings.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Pages.Shared.PartialViews.AddActionNoteModal;
using Server.Pages.Shared.PartialViews.AddCaseDetailUpdateAction;
using Server.Pages.Shared.PartialViews.AddCreateSentralEntryAction;
using Server.Pages.Shared.PartialViews.ConfirmActionUpdateModal;
using Server.Pages.Shared.PartialViews.ConfirmCaseUpdateModal;
using Server.Pages.Shared.PartialViews.ReassignActionToStaffMemberModal;

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

    [ViewData] public string ActivePage => WorkFlowPages.Cases;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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

    public IActionResult OnPostAjaxCaseConfirmation(ConfirmCaseUpdateModalViewModel.UpdateType type)
    {
        ConfirmCaseUpdateModalViewModel viewModel = new(
            Id,
            type);

        return Partial("ConfirmCaseUpdateModal", viewModel);
    }

    public async Task<IActionResult> OnGetCancelCase()
    {
        Result cancelRequest = await _mediator.Send(new CancelCaseCommand(CaseId.FromValue(Id)));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetCompleteCase()
    {
        Result cancelRequest = await _mediator.Send(new CloseCaseCommand(CaseId.FromValue(Id)));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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
        Result cancelRequest = await _mediator.Send(new CancelActionCommand(CaseId.FromValue(Id), ActionId.FromValue(actionId)));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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
        Result noteRequest = await _mediator.Send(new AddActionNoteCommand(CaseId.FromValue(Id), ActionId.FromValue(actionId), note));

        if (noteRequest.IsFailure)
        {
            Error = new()
            {
                Error = noteRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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
        Result reassignRequest = await _mediator.Send(new ReassignActionCommand(CaseId.FromValue(Id), ActionId.FromValue(actionId), staffId));

        if (reassignRequest.IsFailure)
        {
            Error = new()
            {
                Error = reassignRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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

                return BadRequest();
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

                return BadRequest();
                break;
            case nameof(ParentInterviewAction):

                return BadRequest();
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

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSentralEntryActionCommand(CaseId.FromValue(Id), OfferingId.FromValue(viewModel.OfferingId), viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

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

        Result actionRequest = await _mediator.Send(new AddCaseDetailUpdateActionCommand(CaseId.FromValue(Id), staffMemberId, viewModel.Details));

        if (actionRequest.IsFailure)
        {
            Error = new()
            {
                Error = actionRequest.Error,
                RedirectPath = null
            };

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId.FromValue(Id)));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }
}