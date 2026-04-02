namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.Drafts;

using Application.Domains.Messaging.EmergencyConsole.Queries.GetAllEmergencyConsoleMessageTemplates;
using Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplate;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Enums;
using Core.Models.Messaging.Drafts.Errors;
using Core.Models.Messaging.Drafts.Identifiers;
using Core.Models.Messaging.Drafts.Repositories;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.Enums;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.Reflection;

[HasPermission(AuthPermission.Messaging_Email_Send_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMessageDraftRepository _draftRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IMessageDraftRepository draftRepository,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _draftRepository = draftRepository;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_Drafts;
    [ViewData] public string PageTitle => "Message Draft";

    public List<MessageRecipient> Recipients { get; set; } = [];
    public MessageType Type { get; set; }
    public MessageSender Sender { get; set; }
    public string? Subject { get; set; } = string.Empty;
    public string? Body { get; set; } = string.Empty;
    public bool CanSendSms { get; set; } = false;

    public IReadOnlyList<MessageTemplate> Templates { get; set; }

    public IReadOnlyList<MessageSender> EmailSenders { get; set; }
        
    public IReadOnlyList<MessageSender> SmsSenders { get; } =
        typeof(SmsRecipient)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(SmsRecipient))
            .Select(f => (MessageSender)(SmsRecipient)f.GetValue(null)!)
            .Where(s => !string.IsNullOrWhiteSpace(s.Name)) // exclude Unknown
            .ToList();

    private async Task<IReadOnlyList<MessageSender>> GetEmailSenders()
    {
        List<MessageSender> list = [];

        Result<EmailRecipient> recipient = EmailRecipient.Create(_currentUserService.UserName, _currentUserService.EmailAddress);

        if (recipient.IsSuccess)
            list.Add(recipient.Value);

        AuthorizationResult allowAllSenders = await _authorizationService.AuthorizeAsync(User, AuthPermission.Messaging_Email_SendFromAll_Value);
        if (allowAllSenders.Succeeded)
        {
            list.AddRange(typeof(EmailRecipient)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(EmailRecipient))
                .Select(f => (MessageSender)(EmailRecipient)f.GetValue(null)!)
                .ToList());
        }
        else
        {
            list.Add(EmailRecipient.AuroraCollege);
        }
  
        return list.AsReadOnly();
    }

    public async Task OnGet()
    {
        EmailSenders = await GetEmailSenders();

        MessageDraft draft = await _draftRepository.GetDraft(User.GetUserId());

        Recipients = draft.Recipients.ToList();
        Sender = draft.Sender;
        Type = draft.Type;
        Subject = draft.Subject;
        Body = draft.Body;

        AuthorizationResult canSendSms = await _authorizationService.AuthorizeAsync(User, AuthPermission.Messaging_SMS_Send_Value);
        CanSendSms = canSendSms.Succeeded;

        Result<List<MessageTemplate>> templatesRequest = await _mediator.Send(new GetAllEmergencyConsoleMessageTemplatesQuery());

        if (templatesRequest.IsFailure)
        {
            return;
        }

        Templates = templatesRequest.Value;
    }

    public async Task<IActionResult> OnGetClearDraft()
    {
        await _draftRepository.DeleteDraft(User.GetUserId());

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPost(MessagePriority priority = MessagePriority.Normal)
    {
        await _draftRepository.SendDraft(User.GetUserId(), priority);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxLoadTemplate(TemplateId templateId)
    {
        Result<MessageTemplate> templateRequest = await _mediator.Send(new GetEmergencyConsoleMessageTemplateQuery(templateId));

        return templateRequest.IsFailure ? BadRequest() : new JsonResult(templateRequest.Value.Template);
    }

    public async Task<IActionResult> OnPostAjaxAutoSave([FromBody] AutoSaveViewModel vm)
    {
        await _draftRepository.UpdateDraft(
            User.GetUserId(),
            draft =>
            {
                draft.Subject = vm.Subject;
                draft.Body = vm.Body;
            });

        return new OkResult();
    }

    public async Task<IActionResult> OnPostAddRecipient(AddRecipientViewModel vm)
    {
        EmailAddress email = EmailAddress.None;
        PhoneNumber phone = PhoneNumber.Empty;
        bool updated = false;

        Result<EmailAddress> emailAttempt = EmailAddress.Create(vm.Email);
        if (emailAttempt.IsSuccess)
        {
            email = emailAttempt.Value;
            updated = true;
        }

        Result<PhoneNumber> phoneAttempt = PhoneNumber.Create(vm.Phone);
        if (phoneAttempt.IsSuccess)
        {
            phone = phoneAttempt.Value;
            updated = true;
        }

        if (!updated)
            return RedirectToPage();

        MessageRecipient recipient = new MessageRecipient(email, phone, vm.Name);

        await _draftRepository.AddRecipient(recipient, User.GetUserId());

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveRecipient(MessageRecipientId recipientId)
    {
        Result result = await _draftRepository.RemoveRecipient(recipientId, User.GetUserId());

        if (result.IsSuccess)
            return new OkResult();
        
        return BadRequest();
    }

    public async Task<IActionResult> OnPostAjaxChangeType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequest();

        MessageType? messageType = MessageType.FromName(type);

        if (messageType is null)
            return BadRequest();

        await _draftRepository.UpdateDraft(
            User.GetUserId(),
            draft =>
            {
                draft.Type = messageType;
            });

        return new OkResult();
    }

    public async Task<IActionResult> OnPostAjaxAutoSaveSender(
        [FromBody] ChangeTypeViewModel request)
    {
        MessageType? messageType = MessageType.FromValue(request.MessageType);

        if (messageType is null)
            return BadRequest("Invalid message type.");

        Result<MessageSender> sender = Result.Failure<MessageSender>(MessageDraftErrors.InvalidSender);

        if (messageType == MessageType.Email)
        {
            Result<EmailRecipient> emailSender = EmailRecipient.Create(request.SenderName, request.SenderDestination);

            if (emailSender.IsSuccess)
                sender = Result.Success((MessageSender)emailSender.Value);
        }
        else if (messageType == MessageType.SMS)
        {
            if (request.SenderName == SmsRecipient.AuroraNoReply.Name)
                sender = Result.Success((MessageSender)SmsRecipient.AuroraNoReply);
            else
            {
                Result<SmsRecipient> smsSender = SmsRecipient.Create(request.SenderName, request.SenderDestination);

                if (smsSender.IsSuccess)
                    sender = Result.Success((MessageSender)smsSender.Value);
            }
        }

        await _draftRepository.UpdateDraft(User.GetUserId(), d =>
        {
            d.Type = messageType;
            d.Sender = sender.IsSuccess ? sender.Value : null;
        });

        return new OkResult();
    }

    public sealed record ChangeTypeViewModel(
        string MessageType, 
        string SenderName, 
        string SenderDestination);

    public sealed class AutoSaveViewModel
    {
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    public sealed class AddRecipientViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
