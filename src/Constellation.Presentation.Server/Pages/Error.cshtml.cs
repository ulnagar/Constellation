namespace Constellation.Presentation.Server.Pages;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Diagnostics;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ILogger _logger;
    public string RequestId { get; set; }
    public ProblemDetails ProblemDetails { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public ErrorModel(
        ILogger logger)
    {
        _logger = logger.ForContext<ErrorModel>();
    }

    public void OnGet()
    {
        var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionDetails?.Error;

        if (ex != null)
        {
            var title = "An error occured: " + ex.Message;
            var details = ex.ToString();

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
    }
}