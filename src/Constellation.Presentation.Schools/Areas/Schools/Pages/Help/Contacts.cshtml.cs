namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Help;

using Application.Models.Auth;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ContactsModel : BasePageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ContactsModel(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ContactsModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Help;

    public void OnGet()
    {
        _logger.Information("Requested to retrieve contacts help page by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
    }
}