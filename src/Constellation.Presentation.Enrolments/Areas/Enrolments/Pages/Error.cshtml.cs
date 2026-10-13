namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages;

using Constellation.Core.Abstractions.Services;
using Core.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Extensions;
using Serilog;
using System.Diagnostics;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location =ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel : BasePageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ErrorModel(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForEnrolmentsPortal()
            .ForContext<ErrorModel>();
    }


    public async Task OnGet()
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        IExceptionHandlerPathFeature? feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        string? failedPath = feature?.Path;
        Exception? exception = feature?.Error;
        string referrer = Request.Headers.Referer.ToString();

        _logger
            .ForContext(nameof(EmailAddress), _currentUserService.EmailAddress)
            .ForContext("Failed Path", failedPath)
            .ForContext("Request Id", requestId)
            .ForContext("Referrer", referrer)
            .Error(exception, "User experienced Hard Error");
    }
}