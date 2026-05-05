namespace Constellation.Presentation.Server.Areas.Admin.Pages.Hosting.Livestreams;

using Application.Common.PresentationModels;
using Application.Domains.Hosting.Commands.UpsertLivestream;
using Application.Domains.Hosting.Queries.GetLivestream;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Domains.Hosting.Commands.UpsertNewsletter;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Hosting.Errors;
using Core.Models.Hosting;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

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

    [ViewData] public string ActivePage => Models.ActivePage.Hosting_Livestreams;
    [ViewData] public string PageTitle => "Livestreams";

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string EmbedCode { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly StartsOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ExpiresOn { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    public async Task OnGet()
    {
        if (Id is null)
            return;

        Result<Livestream> livestream = await _mediator.Send(new GetLivestreamQuery(Id.Value));

        if (livestream.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), livestream.Error, true)
                .Warning("Failed to retrieve Livestream for edit by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                livestream.Error,
                _linkGenerator.GetPathByPage("/Hosting/Livestreams/Index", values: new { area = "Admin" }));

            return;
        }

        Name = livestream.Value.Name;
        EmbedCode = livestream.Value.EmbedCode;
        Description = livestream.Value.Description;
        StartsOn = livestream.Value.StartsOn;
        ExpiresOn = livestream.Value.ExpiresOn;
    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            _logger
                .ForContext(nameof(Error), LivestreamErrors.MustIncludeName, true)
                .Warning("Failed to save Livestream by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(LivestreamErrors.MustIncludeName, null);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(EmbedCode))
        {
            _logger
                .ForContext(nameof(Error), LivestreamErrors.MustIncludeEmbedCode, true)
                .Warning("Failed to save Livestream by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(LivestreamErrors.MustIncludeEmbedCode, null);

            return Page();
        }

        UpsertLivestreamCommand command = new(
            Id,
            Name,
            EmbedCode,
            Description,
            StartsOn,
            ExpiresOn);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to save Livestream by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error, null);

            return Page();
        }

        return RedirectToPage("/Hosting/Livestreams/Index", new { area = "Admin" });
    }
}