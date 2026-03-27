namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.Drafts;

using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Identifiers;
using Core.Models.Messaging.Drafts.Repositories;
using Core.Models.Messaging.Enums;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Messaging_View_Value)]
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
    public string? Subject { get; set; } = string.Empty;
    public string? Body { get; set; } = string.Empty;

    public async Task OnGet()
    {
        MessageDraft draft = await _draftRepository.GetDraft(User.GetUserId());

        Recipients = draft.Recipients.ToList();
        Type = draft.Type;
        Subject = draft.Subject;
        Body = draft.Body;
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
