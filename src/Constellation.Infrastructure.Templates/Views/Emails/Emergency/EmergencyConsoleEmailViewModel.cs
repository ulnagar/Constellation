namespace Constellation.Infrastructure.Templates.Views.Emails.Emergency;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class EmergencyConsoleEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Emergency/EmergencyConsoleEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public required string Message { get; set; }
}
