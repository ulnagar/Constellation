namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency;

using Application.Common.PresentationModels;
using Application.Domains.EmergencyConsole.Commands.SendEmergencyMessage;
using Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplates;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[HasPermission(AuthPermission.Admin_EmergencyConsole_Edit_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Console;

    [ViewData]
    public string PageTitle => "Emergency Console";


    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public MessageType Type { get; set; } = MessageType.Email;

    [BindProperty]
    public List<RecipientGroup>? RecipientGroups { get; set; } = [];

    [BindProperty]
    public List<AlertRecipient>? Recipients { get; set; } = [];

    [BindProperty]
    public TemplateId? TemplateId { get; set; } = TemplateId.Empty;

    [BindProperty]
    public string? Message { get; set; } = string.Empty;

    public List<MessageTemplate> Templates { get; set; } = [];

    public async Task OnGet()
    {
        await PreparePage();
    }

    private async Task PreparePage()
    {
        Result<List<MessageTemplate>> templates = await _mediator.Send(new GetEmergencyConsoleMessageTemplatesQuery());

        if (templates.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(templates.Error);

            return;
        }

        Message ??= string.Empty;

        Templates = templates.Value;
    }

    public async Task<IActionResult> OnPostSend()
    {
        if (RecipientGroups.Count == 0 && Recipients.Count == 0)
            ModelState.AddModelError(nameof(Recipients), "Must include at least one recipient or group");

        if (string.IsNullOrWhiteSpace(Message))
            ModelState.AddModelError(nameof(Message), "Must include a message to send");

        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        Result result = await _mediator.Send(new SendEmergencyMessageCommand(RecipientGroups, Recipients, Type, Message));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);
            
            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLoadTemplate()
    {
        await PreparePage();

        var chosenTemplate = Templates.FirstOrDefault(entry => entry.Id == TemplateId);

        if (chosenTemplate is null)
            return Page();

        Type = chosenTemplate.TemplateType;
        Message = chosenTemplate.Template;

        return Page();
    }
}