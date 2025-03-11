namespace Constellation.Presentation.Parents.Areas.Parents.Pages.ThirdParty;

using Application.Common.PresentationModels;
using Application.ThirdPartyConsent.GetApplicationsWithoutRequiredConsent;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Presentation.Shared.Helpers.Logging;
using Contacts;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
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
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.ThirdParty;

    public List<ApprovedApplicationResponse> Applications { get; set; } = new();

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve approved Applications by user {User}", _currentUserService.UserName);

        Result<List<ApprovedApplicationResponse>> applications = await _mediator.Send(new GetApplicationsWithoutRequiredConsentQuery());

        if (applications.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applications.Error, true)
                .Warning("Failed to retrieve approved Applications by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                applications.Error,
                _linkGenerator.GetPathByPage("/Index", values: new { area = "Parents" }));

            return;
        }

        Applications = applications.Value.OrderBy(app => app.Name).ToList();
    }
}