namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Help;

using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Extensions;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class AwardsModel : BasePageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AwardsModel(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AwardsModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Help;

    public void OnGet()
    {
        _logger.Information("Requested to retrieve awards help page by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
    }
}