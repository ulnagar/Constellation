namespace Constellation.Presentation.Server.Pages;

using Core.Abstractions.Services;
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

    public ErrorModel(
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ErrorModel>();
    }

    public int StatusCode { get; private set; }
    public string? RequestId { get; private set; }
    public string Code { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string BadgeText { get; private set; }

    public void OnGet(int? statusCode = null)
    {
        if (statusCode is null)
            StatusCode = HttpContext.Request.Query.ContainsKey("ReturnUrl") ? 403 : 404;
        else
            StatusCode = statusCode.Value;

        Code = StatusCode.ToString();
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        switch (Code)
        {
            case "403":
                Title = "Access denied";
                Description = "You don't have permission to access this area. Contact the Technology Support Team if you believe this is a mistake.";
                BadgeText = "Error 403";
                break;

            case "422":
                Title = "Invalid Id";
                Description = "You have supplied an invalid Application Id. The record does not exist, or has been deleted.";
                BadgeText = "Error 422";
                break;

            case "500":
                Title = "Something went wrong";
                Description = "An unexpected server error occurred. Our team has been notified and is investigating.";
                BadgeText = "Error 500";
                break;

            case "503":
                Title = "Scheduled maintenance";
                Description = "Constellation is briefly offline for scheduled maintenance. We'll have everything back up shortly. Thank you for your patience.";
                BadgeText = "Maintenance";
                break;

            default:
                Title = "Page not found";
                Description = "This page doesn't exist or may have been moved. Check the URL or return to a safe place.";
                BadgeText = "Error 404";
                break;
        }

        _logger
            .ForContext(nameof(_currentUserService.UserName), _currentUserService.UserName)
            .ForContext(nameof(_currentUserService.EmailAddress), _currentUserService.EmailAddress)
            .ForContext(nameof(HttpContext.TraceIdentifier), RequestId)
            .ForContext(nameof(StatusCode), StatusCode)
            .Error("User experienced hard error");
    }
}