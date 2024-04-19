namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Actions;

using Application.Models.Auth;
using Application.WorkFlows.UpdateConfirmSentralEntryAction;
using Application.WorkFlows.UpdateCreateSentralEntryAction;
using Application.WorkFlows.UpdateParentInterviewAction;
using Application.WorkFlows.UpdatePhoneParentAction;
using Application.WorkFlows.UpdateSendEmailAction;
using BaseModels;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Modules.GetTrainingModuleEditContext;
using Constellation.Application.WorkFlows.GetCaseById;
using Constellation.Core.Models.Training.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Cms;
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

            return Page();
        }

        return RedirectToPage("/WorkFlows/Details", new { area = "SchoolAdmin", Id = CaseId.Value });
    }
}