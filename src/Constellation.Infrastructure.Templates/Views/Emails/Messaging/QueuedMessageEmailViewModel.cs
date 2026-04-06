namespace Constellation.Infrastructure.Templates.Views.Emails.Messaging;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class QueuedMessageEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Messaging/QueuedMessageEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public required string Message { get; set; }
}
