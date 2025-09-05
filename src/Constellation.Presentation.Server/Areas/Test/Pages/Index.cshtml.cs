namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.MeritAwards.Nominations.Commands.SendParentNotifications;
using Application.Domains.MeritAwards.Nominations.Commands.SendSchoolNotifications;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Awards.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {

    }

}