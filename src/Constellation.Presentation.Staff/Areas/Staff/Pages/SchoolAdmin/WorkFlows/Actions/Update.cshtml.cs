namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Actions;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.WorkFlows.UpdateConfirmSentralEntryAction;
using Application.WorkFlows.UpdateCreateSentralEntryAction;
using Application.WorkFlows.UpdateParentInterviewAction;
using Application.WorkFlows.UpdatePhoneParentAction;
using Application.WorkFlows.UpdateSendEmailAction;
using Application.WorkFlows.UpdateSentralIncidentStatusAction;
using Application.WorkFlows.UpdateUploadTrainingCertificateAction;
using Constellation.Application.DTOs;
using Constellation.Application.WorkFlows.GetCaseById;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.Components.ActionUpdateForm;

[Authorize(Policy = AuthPolicies.CanEditWorkFlowAction)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpdateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpdateModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;
    [ViewData] public string PageTitle { get; set; } = "WorkFlow Action Update";

    [ModelBinder(typeof(ConstructorBinder))]
    [BindProperty(SupportsGet = true)]
    public CaseId CaseId { get; set; }

    [ModelBinder(typeof(ConstructorBinder))]
    [BindProperty(SupportsGet = true)]
    public ActionId ActionId { get; set; }

    public CaseDetailsResponse Case { get; set; }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve WorkFlow Action with id {Id} for update by user {User}", ActionId, _currentUserService.UserName);

        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve WorkFlow Action with id {Id} for update by user {User}", CaseId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Index", values: new { area = "Staff" }));

            return;
        }

        Case = request.Value;
    }

    public Task OnGet() => PreparePage();

    public async Task<IActionResult> OnPostUpdateSentralEntryAction(CreateSentralEntryActionViewModel viewModel)
    {
        UpdateCreateSentralEntryActionCommand command = new(CaseId, ActionId, viewModel.NotRequired, viewModel.IncidentNumber);

        _logger
            .ForContext(nameof(UpdateCreateSentralEntryActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }
        
        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateConfirmEntryAction(ConfirmSentralEntryActionViewModel viewModel)
    {
        UpdateConfirmSentralEntryActionCommand command = new(CaseId, ActionId, viewModel.Confirmed);

        _logger
            .ForContext(nameof(UpdateConfirmSentralEntryActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdatePhoneParentAction(PhoneParentActionViewModel viewModel)
    {
        UpdatePhoneParentActionCommand command = new(CaseId, ActionId, viewModel.ParentName, viewModel.ParentPhoneNumber, viewModel.DateOccurred, viewModel.IncidentNumber);

        _logger
            .ForContext(nameof(UpdatePhoneParentActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();
            
            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateParentInterviewAction(ParentInterviewActionViewModel viewModel)
    {
        List<InterviewAttendee> attendees = new();

        foreach (ParentInterviewActionViewModel.Attendee attendee in viewModel.Attendees)
            attendees.Add(InterviewAttendee.Create(ActionId, attendee.Name, attendee.Notes));

        UpdateParentInterviewActionCommand command = new(CaseId, ActionId, attendees, viewModel.DateOccurred, viewModel.IncidentNumber);

        _logger
            .ForContext(nameof(UpdateParentInterviewActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateIncidentStatusEntryAction(SentralIncidentStatusActionViewModel viewModel)
    {
        if (viewModel.Status == "Not Completed" && viewModel.IncidentNumber == 0)
        {
            ModalContent = new ErrorDisplay(ActionErrors.UpdateIncidentNumberZero);

            await PreparePage();

            return Page();
        }

        bool markResolved = viewModel.Status == "Resolved";
        bool markNotCompleted = viewModel.Status == "Not Completed";

        UpdateSentralIncidentStatusActionCommand command = new(CaseId, ActionId, markResolved, markNotCompleted, viewModel.IncidentNumber);

        _logger
            .ForContext(nameof(UpdateSentralIncidentStatusActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateSendEmailAction(SendEmailActionViewModel viewModel)
    {
        List<FileDto> files = new();

        foreach (IFormFile attachment in viewModel.Attachments)
        {
            FileDto file = new()
            {
                FileName = attachment.FileName,
                FileType = attachment.ContentType
            };

            try
            {
                await using MemoryStream target = new();
                await attachment.CopyToAsync(target);
                file.FileData = target.ToArray();
                files.Add(file);
            }
            catch (Exception ex)
            {
                ModalContent = new ErrorDisplay(new("Email.Attachment.Failure", $"Failed to read attachment: {file.FileName}"));

                await PreparePage();

                return Page();
            }
        }

        List<EmailRecipient> recipients = new();

        foreach (KeyValuePair<string, string> recipient in viewModel.Recipients)
        {
            Result<EmailRecipient> entry = EmailRecipient.Create(recipient.Key, recipient.Value);

            if (entry.IsFailure)
            {
                ModalContent = new ErrorDisplay(new("Email.Recipient.Failure", $"Failed to add recipient: {recipient.Key} ({recipient.Value})"));

                await PreparePage();

                return Page();
            }

            recipients.Add(entry.Value);
        }

        Result<EmailRecipient> sender = EmailRecipient.Create(viewModel.SenderName, viewModel.SenderEmail);

        if (sender.IsFailure)
        {
            ModalContent = new ErrorDisplay(new("Email.Sender.Failure", $"Failed to add sender: {viewModel.SenderName} ({viewModel.SenderEmail})"));

            await PreparePage();

            return Page();
        }

        UpdateSendEmailActionCommand command = new(CaseId, ActionId, recipients, sender.Value, viewModel.Subject, viewModel.Body, files);

        _logger
            .ForContext(nameof(UpdateSendEmailActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateUploadTrainingCertificateAction()
    {
        UpdateUploadTrainingCertificateActionCommand command = new(CaseId, ActionId);

        _logger
            .ForContext(nameof(UpdateUploadTrainingCertificateActionCommand), command, true)
            .Information("Requested to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to update WorkFlow Action with id {Id} by user {User}", ActionId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(attempt.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }
}