namespace Constellation.Presentation.Server.Areas.Admin.Pages.Hosting;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Hosting.Queries.GetAllNewsletters;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[HasPermission(AuthPermission.Admin_Hosting_View_Value)]
public class NewslettersModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public NewslettersModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<NewslettersModel>();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Hosting_Newsletters;
    [ViewData] public string PageTitle => "Newsletters";

    public List<Newsletter> Newsletters { get; set; } = [];

    public async Task OnGet()
    {
        var newsletters = await _mediator.Send(new GetAllNewslettersQuery());

        if (newsletters.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), newsletters.Error, true)
                .Warning("Failed to retrieve list of Newsletters by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(newsletters.Error, null);

            return;
        }

        Newsletters = newsletters.Value;
    }
}

