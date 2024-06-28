namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Actions;

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
using Presentation.Shared.Helpers.ModelBinders;
using Presentation.Shared.Pages.Shared.Components.ActionUpdateForm;

[Authorize(Policy = AuthPolicies.CanEditWorkFlowAction)]
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public CaseId CaseId { get; set; }

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public ActionId ActionId { get; set; }

    public CaseDetailsResponse Case { get; set; }

    private async Task PreparePage()
    {
        Result<CaseDetailsResponse> request = await _mediator.Send(new GetCaseByIdQuery(CaseId));

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

    public Task OnGet() => PreparePage();

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

            await PreparePage();

            return Page();
        }
        
        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
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

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
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

        Result attempt = await _mediator.Send(new UpdateParentInterviewActionCommand(CaseId, ActionId, attendees, viewModel.DateOccurred, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateIncidentStatusEntryAction(SentralIncidentStatusActionViewModel viewModel)
    {
        if (viewModel.Status == "Not Completed" && viewModel.IncidentNumber == 0)
        {
            Error = new()
            {
                Error = ActionErrors.UpdateIncidentNumberZero,
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        bool markResolved = viewModel.Status == "Resolved";
        bool markNotCompleted = viewModel.Status == "Not Completed";

        Result attempt = await _mediator.Send(new UpdateSentralIncidentStatusActionCommand(CaseId, ActionId, markResolved, markNotCompleted, viewModel.IncidentNumber));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

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
                Error = new()
                {
                    Error = new("Email.Attachment.Failure", $"Failed to read attachment: {file.FileName}"),
                    RedirectPath = null
                };

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
                Error = new()
                {
                    Error = new("Email.Recipient.Failure", $"Failed to add recipient: {recipient.Key} ({recipient.Value})"),
                    RedirectPath = null
                };

                await PreparePage();

                return Page();
            }

            recipients.Add(entry.Value);
        }

        Result<EmailRecipient> sender = EmailRecipient.Create(viewModel.SenderName, viewModel.SenderEmail);

        if (sender.IsFailure)
        {
            Error = new()
            {
                Error = new("Email.Sender.Failure", $"Failed to add sender: {viewModel.SenderName} ({viewModel.SenderEmail})"),
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        Result attempt = await _mediator.Send(new UpdateSendEmailActionCommand(CaseId, ActionId, recipients, sender.Value, viewModel.Subject, viewModel.Body, files));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }

    public async Task<IActionResult> OnPostUpdateUploadTrainingCertificateAction()
    {
        Result attempt = await _mediator.Send(new UpdateUploadTrainingCertificateActionCommand(CaseId, ActionId));

        if (attempt.IsFailure)
        {
            Error = new()
            {
                Error = attempt.Error,
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/WorkFlows/Details", new { area = "Staff", Id = CaseId.Value });
    }
}