namespace Constellation.Presentation.Server.Pages;

using Core.Abstractions.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Diagnostics;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;
    public string RequestId { get; set; }
    public ProblemDetails ProblemDetails { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public ErrorModel(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger.ForContext<ErrorModel>();
    }

    public void OnGet()
    {
        IExceptionHandlerFeature exceptionDetails = HttpContext.Features.Get<IExceptionHandlerFeature>();
        Exception ex = exceptionDetails?.Error;

        if (ex != null)
        {
            string title = "An error occured: " + ex.Message;
            string details = ex.ToString();

            ProblemDetails = new ProblemDetails
            {
                Status = 500,
                Title = title,
                Detail = details
            };

            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            if (RequestId != null)
                ProblemDetails.Extensions["traceId"] = RequestId;
        }

        _logger
            .ForContext(nameof(_currentUserService.UserName), _currentUserService.UserName)
            .ForContext(nameof(_currentUserService.EmailAddress), _currentUserService.EmailAddress)
            .ForContext(nameof(ProblemDetails), ProblemDetails, true)
            .Error("User experienced hard error");
    }
}