namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;
using Application.Domains.WorkFlows.Commands.AddActionNote;
using Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;
using Application.Domains.WorkFlows.Commands.AddParentInterviewAction;
using Application.Domains.WorkFlows.Commands.AddPhoneParentAction;
using Application.Domains.WorkFlows.Commands.AddSendEmailAction;
using Application.Domains.WorkFlows.Commands.AddSentralEntryAction;
using Application.Domains.WorkFlows.Commands.AddSentralIncidentStatusAction;
using Application.Domains.WorkFlows.Commands.CancelAction;
using Application.Domains.WorkFlows.Commands.ReassignAction;
using Application.Domains.WorkFlows.Commands.UpdateCaseStatus;
using Application.Domains.WorkFlows.Queries.GetCaseById;
using Application.Models.Auth;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Core.Abstractions.Services;
using Core.Models.Offerings.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;
    [ViewData] public string PageTitle => "WorkFlow Case Details";

    [BindProperty(SupportsGet = true)]
    public CaseId Id { get; set; }

    public CaseDetailsResponse Case { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details of Case with Id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve details of Case with Id {Id} by user {User}", Id, _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Index", values: new { area = "Staff" }));

            return;
        }

        Case = request.Value;
    }

    public async Task<IActionResult> OnPostUpdateStatus(
        [ModelBinder(typeof(BaseFromValueBinder))] CaseStatus status)
    {
        UpdateCaseStatusCommand command = new(Id, status);

        _logger
            .ForContext(nameof(UpdateCaseStatusCommand), command, true)
            .Information("Requested to update status of Case with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result action = await _mediator.Send(command);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Failed to update status of Case with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(action.Error);

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

    public async Task<IActionResult> OnGetCancelAction(ActionId actionId)
    {
        CancelActionCommand command = new(Id, actionId);

        _logger
            .ForContext(nameof(CancelActionCommand), command, true)
            .Information("Requested to cancel Action in Case by user {User}", _currentUserService.UserName);

        Result cancelRequest = await _mediator.Send(command);

        if (cancelRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), cancelRequest, true)
                .Warning("Failed to cancel Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(cancelRequest.Error);

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

    public async Task<IActionResult> OnPostAddActionNote(
        ActionId actionId, 
        string note)
    {
        AddActionNoteCommand command = new(Id, actionId, note);

        _logger
            .ForContext(nameof(AddActionNoteCommand), command, true)
            .Information("Requested to cancel Action in Case by user {User}", _currentUserService.UserName);

        Result noteRequest = await _mediator.Send(command);

        if (noteRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), noteRequest.Error, true)
                .Warning("Failed to cancel Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(noteRequest.Error);

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
        Dictionary<StaffId, string> staffList = new();

        Result<Dictionary<StaffId, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffListRequest.Error, true)
                .Warning("Failed to retrieve list of staff by user {User}", _currentUserService.UserName);
        }
        else
        {
            staffList = staffListRequest.Value;
        }

        ReassignActionToStaffMemberModalViewModel viewModel = new()
        {
            ActionId = ActionId.FromValue(actionId),
            StaffMembers = staffList
        };

        return Partial("ReassignActionToStaffMemberModal", viewModel);
    }

    public async Task<IActionResult> OnPostReassignAction(
        ActionId actionId, 
        StaffId staffId)
    {
        ReassignActionCommand command = new(Id, actionId, staffId);

        _logger
            .ForContext(nameof(ReassignActionCommand), command, true)
            .Information("Requested to reassign Action in Case by user {User}", _currentUserService.UserName);

        Result reassignRequest = await _mediator.Send(command);

        if (reassignRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reassignRequest.Error, true)
                .Warning("Failed to reassign Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(reassignRequest.Error);

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
        Dictionary<StaffId, string> staffResult = new();

        Result<Dictionary<StaffId, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffListRequest.Error, true)
                .Warning("Failed to retrieve list of staff by user {User}", _currentUserService.UserName);
        }
        else
        {
            staffResult = staffListRequest.Value;
        }
        
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
        if (viewModel.StaffId == StaffId.Empty)
        {
            ModalContent = ErrorDisplay.Create(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddSentralEntryActionCommand command = new(Id, OfferingId.FromValue(viewModel.OfferingId), viewModel.StaffId);

        _logger
            .ForContext(nameof(AddSentralEntryActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

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
        string claimStaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;
        Guid guidStaffId = Guid.Parse(claimStaffId);
        StaffId staffId = StaffId.FromValue(guidStaffId);

        GetStaffByIdQuery query = new(staffId);

        Result<StaffResponse> staffMember = await _mediator.Send(query);

        if (staffMember.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffMember.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(staffMember.Error);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddCaseDetailUpdateActionCommand command = new(Id, staffMember.Value.EmployeeId, viewModel.Details);

        _logger
            .ForContext(nameof(AddCaseDetailUpdateActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

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
        if (viewModel.StaffId == StaffId.Empty)
        {
            ModalContent = ErrorDisplay.Create(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddParentInterviewActionCommand command = new(Id, viewModel.StaffId);

        _logger
            .ForContext(nameof(AddParentInterviewActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

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
        if (viewModel.StaffId == StaffId.Empty)
        {
            ModalContent = ErrorDisplay.Create(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddPhoneParentActionCommand command = new(Id, viewModel.StaffId);

        _logger
            .ForContext(nameof(AddPhoneParentActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

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
        if (viewModel.StaffId == StaffId.Empty)
        {
            ModalContent = ErrorDisplay.Create(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddSendEmailActionCommand command = new(Id, viewModel.StaffId);

        _logger
            .ForContext(nameof(AddSendEmailActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

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
        if (viewModel.StaffId == StaffId.Empty)
        {
            ModalContent = ErrorDisplay.Create(ActionErrors.AssignStaffNull);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        AddSentralIncidentStatusActionCommand command = new(Id, viewModel.StaffId);

        _logger
            .ForContext(nameof(AddSentralIncidentStatusActionCommand), command, true)
            .Information("Requested to add Action in Case by user {User}", _currentUserService.UserName);

        Result actionRequest = await _mediator.Send(command);

        if (actionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), actionRequest.Error, true)
                .Warning("Failed to add Action in Case by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(actionRequest.Error);

            Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(Id));

            if (request.IsFailure)
                return RedirectToPage();

            Case = request.Value;

            return Page();
        }

        return RedirectToPage();
    }
}