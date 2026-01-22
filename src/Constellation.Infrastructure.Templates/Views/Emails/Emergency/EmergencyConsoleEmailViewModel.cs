namespace Constellation.Infrastructure.Templates.Views.Emails.Emergency;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class EmergencyConsoleEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/Emergency/EmergencyConsoleEmail.cshtml";

    public string Message { get; set; }
}
