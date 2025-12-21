namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency.Templates;

using Application.Common.PresentationModels;
using Application.Domains.EmergencyConsole.Commands.CreateNewEmergencyConsoleMessageTemplate;
using Application.Domains.EmergencyConsole.Commands.DeleteEmergencyConsoleMessageTemplate;
using Application.Domains.EmergencyConsole.Commands.UpdateEmergencyConsoleMessageTemplate;
using Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplate;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanUseEmergencyConsole)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>();
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Templates;

    [ViewData]
    public string PageTitle => "Templates";

    [BindProperty(SupportsGet = true)]
    public TemplateId Id { get; set; } = TemplateId.Empty;

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public MessageType Type { get; set; } = MessageType.Email;

    [BindProperty]
    [Required]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Template { get; set; } = string.Empty;


    public async Task OnGet()
    {
        if (Id == TemplateId.Empty)
            return;

        GetEmergencyConsoleMessageTemplateQuery query = new(Id);

        Result<MessageTemplate> template = await _mediator.Send(query);

        if (template.IsFailure)
        {
            _logger
                .ForContext(nameof(GetEmergencyConsoleMessageTemplateQuery), query, true)
                .ForContext(nameof(Error), template.Error, true)
                .Warning("Failed to retrieve MessageEvent Template for edit by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                template.Error,
                _linkGenerator.GetPathByPage("/Emergency/Templates/Index", values: new { area = "Admin" }));

            return;
        }

        Type = template.Value.TemplateType;
        Name = template.Value.Name;
        Template = template.Value.Template;
    }

    public async Task<IActionResult> OnGetDelete()
    {
        DeleteEmergencyConsoleMessageTemplateCommand command = new(Id);

        _logger
            .ForContext(nameof(DeleteEmergencyConsoleMessageTemplateCommand), command, true)
            .Information("Requested to delete MessageEvent Template by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Information("Requested to delete MessageEvent Template by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Emergency/Templates/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage("/Emergency/Templates/Index", new { area = "Admin" });
    }

    public async Task<IActionResult> OnPostAjaxDelete()
    {
        return Partial("ConfirmMessageTemplateDelete");
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == TemplateId.Empty)
        {
            CreateNewEmergencyConsoleMessageTemplateCommand command = new(Type, Name, Template);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateNewEmergencyConsoleMessageTemplateCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new MessageEvent Template for edit by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return Page();
            }

            return RedirectToPage("/Emergency/Templates/Index", new { area = "Admin" });
        }
        else
        {
            UpdateEmergencyConsoleMessageTemplateCommand command = new(Id, Name, Template);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateEmergencyConsoleMessageTemplateCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to update MessageEvent Template for edit by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return Page();
            }

            return RedirectToPage("/Emergency/Templates/Index", new { area = "Admin" });
        }
    }
}