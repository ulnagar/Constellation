namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Common.PresentationModels;
using Application.Features.Common.Queries;
using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.WorkFlows.AddActionNote;
using Application.WorkFlows.AddCaseDetailUpdateAction;
using Application.WorkFlows.AddParentInterviewAction;
using Application.WorkFlows.AddPhoneParentAction;
using Application.WorkFlows.AddSendEmailAction;
using Application.WorkFlows.AddSentralEntryAction;
using Application.WorkFlows.AddSentralIncidentStatusAction;
using Application.WorkFlows.CancelAction;
using Application.WorkFlows.GetCaseById;
using Application.WorkFlows.ReassignAction;
using Application.WorkFlows.UpdateCaseStatus;
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
using Shared.PartialViews.AddActionNoteModal;
using Shared.PartialViews.AddCaseDetailUpdateAction;
using Shared.PartialViews.AddCreateSentralEntryAction;
using Shared.PartialViews.AddParentInterviewAction;
using Shared.PartialViews.AddPhoneParentAction;
using Shared.PartialViews.AddSendEmailAction;
using Shared.PartialViews.AddSentralIncidentStatusAction;
using Shared.PartialViews.ConfirmActionUpdateModal;
using Shared.PartialViews.ReassignActionToStaffMemberModal;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public CaseId Id { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Index", values: new { area = "Staff" }));

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
            ModalContent = new ErrorDisplay(action.Error);

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
            ModalContent = new ErrorDisplay(cancelRequest.Error);

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
            ModalContent = new ErrorDisplay(noteRequest.Error);

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
            ModalContent = new ErrorDisplay(reassignRequest.Error);

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
            case nameof(SentralIncidentStatusAction):
                AddSentralIncidentStatusActionViewModel incidentViewModel = new()
                {
                    StaffMembers = staffResult
                };

                return Partial("AddSentralIncidentStatusAction", incidentViewModel);
                break;
            default:
                return BadRequest();
        }
    }

    public async Task<IActionResult> OnPostNewSentralAction(AddCreateSentralEntryActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            ModalContent = new ErrorDisplay(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSentralEntryActionCommand(Id, OfferingId.FromValue(viewModel.OfferingId), viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(actionRequest.Error);

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
            ModalContent = new ErrorDisplay(actionRequest.Error);

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
            ModalContent = new ErrorDisplay(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddParentInterviewActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(actionRequest.Error);

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
            ModalContent = new ErrorDisplay(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddPhoneParentActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(actionRequest.Error);

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
            ModalContent = new ErrorDisplay(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSendEmailActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(actionRequest.Error);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewIncidentAction(AddSentralIncidentStatusActionViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            ModalContent = new ErrorDisplay(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        Result actionRequest = await _mediator.Send(new AddSentralIncidentStatusActionCommand(Id, viewModel.StaffId));

        if (actionRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(actionRequest.Error);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }
}