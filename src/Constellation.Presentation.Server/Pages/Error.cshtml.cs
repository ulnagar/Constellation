using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Constellation.Presentation.Server.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public ProblemDetails ProblemDetails { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
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
}
