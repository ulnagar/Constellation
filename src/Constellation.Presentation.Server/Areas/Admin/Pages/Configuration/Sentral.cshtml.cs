namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Core.Models.AppSettings.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class SentralModel : BasePageModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public SentralModel(
        IAppSettingsService appSettings,
        ISender mediator,
        ILogger logger)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _logger = logger
            .ForContext<SentralModel>();
    }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;

    public List<SentralConfiguration> Configurations { get; set; } = [];

    public async Task OnGet()
    {
        Configurations = await _appSettings.Sentral();
    }

    public async Task<IActionResult> OnPostSave(
        SentralPath type,
        string path)
    {
        SentralConfiguration configuration = new(
            type,
            path);

        await _appSettings.Sentral(configuration);

        return RedirectToPage();
    }
}