namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class AuthenticationModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public AuthenticationModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<AuthenticationModel>();
    }
    
    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    [BindProperty] public bool LoginEnabled { get; set; }
    [BindProperty] public bool SSOEnabled { get; set; }
    
    public async Task OnGet()
    {
        AuthenticationConfiguration? configuration = await _appSettings.Authentication();

        if (configuration is not null)
        {
            LoginEnabled = configuration.LoginEnabled;
            SSOEnabled = configuration.SSOEnabled;
        }
    }
    
    public async Task<IActionResult> OnPostSave()
    {
        AuthenticationConfiguration configuration = new(
            LoginEnabled,
            SSOEnabled);

        await _appSettings.Authentication(configuration);

        return RedirectToPage("/Configuration/Index", new { area = "Admin" });
    }
}