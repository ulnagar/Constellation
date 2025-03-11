namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Help;

using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class AwardsModel : BasePageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AwardsModel(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory,
        ICurrentUserService currentUserService,
        ILogger logger)
        : base(httpContextAccessor, serviceFactory)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AwardsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Help;

    public void OnGet()
    {
        _logger.Information("Requested to retrieve awards help page by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
    }
}