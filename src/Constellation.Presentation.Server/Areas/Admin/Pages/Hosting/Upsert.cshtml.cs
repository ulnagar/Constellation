namespace Constellation.Presentation.Server.Areas.Admin.Pages.Hosting;

using Application.Domains.Hosting.Commands.UpsertNewsletter;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Hosting.Queries.GetNewsletter;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Models.Hosting.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[HasPermission(AuthPermission.Admin_Hosting_Edit_Value)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<UpsertModel>();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Hosting_Newsletters;
    [ViewData] public string PageTitle => "Newsletters";

    [BindProperty(SupportsGet = true)]
    public int? Issue { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string EmbedCode { get; set; } = string.Empty;

    public async Task OnGet()
    {
        if (!Issue.HasValue)
            return;

        Result<Newsletter> newsletter = await _mediator.Send(new GetNewsletterQuery(Issue.Value));

        if (newsletter.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), newsletter.Error, true)
                .Warning("Failed to retrieve Newsletter for edit by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                newsletter.Error, 
                _linkGenerator.GetPathByPage("/Hosting/Newsletters", values: new { area = "Admin"}));

            return;
        }

        Name = newsletter.Value.Name;
        EmbedCode = newsletter.Value.EmbedCode;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!Issue.HasValue)
        {
            _logger
                .ForContext(nameof(Error), NewsletterErrors.InvalidIssueNumber, true)
                .Warning("Failed to save Newsletter by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(NewsletterErrors.InvalidIssueNumber, null);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            _logger
                .ForContext(nameof(Error), NewsletterErrors.MustIncludeName, true)
                .Warning("Failed to save Newsletter by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(NewsletterErrors.MustIncludeName, null);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(EmbedCode))
        {
            _logger
                .ForContext(nameof(Error), NewsletterErrors.MustIncludeEmbedCode, true)
                .Warning("Failed to save Newsletter by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(NewsletterErrors.MustIncludeEmbedCode, null);

            return Page();
        }

        UpsertNewsletterCommand command = new UpsertNewsletterCommand(
            Issue.Value,
            Name,
            EmbedCode);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to save Newsletter by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error, null);

            return Page();
        }

        return RedirectToPage("/Hosting/Newsletters", new { area = "Admin" });
    }
}
