namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Help;

using Application.Models.Auth;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory,
        ICurrentUserService currentUserService,
        ILogger logger)
        : base(httpContextAccessor, serviceFactory)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Help;

    public void OnGet()
    {
        _logger.Information("Requested to retrieve help index page by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
    }
}