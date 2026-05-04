namespace Constellation.Presentation.Server.Areas.Admin.Pages.Hosting.Livestreams;

using Application.Domains.Hosting.Queries.GetAllLivestreams;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Hosting;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Hosting_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Hosting_Livestreams;
    [ViewData] public string PageTitle => "Livestreams";

    public List<Livestream> Livestreams { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<Livestream>> livestreams = await _mediator.Send(new GetAllLivestreamsQuery());

        if (livestreams.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), livestreams.Error, true)
                .Warning("Failed to retrieve list of Livestreams by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(livestreams.Error, null);

            return;
        }

        Livestreams = livestreams.Value;
    }
}