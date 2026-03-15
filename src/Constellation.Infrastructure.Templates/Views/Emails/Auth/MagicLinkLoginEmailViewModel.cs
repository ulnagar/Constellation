namespace Constellation.Infrastructure.Templates.Views.Emails.Auth;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class MagicLinkLoginEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Auth/MagicLinkLoginEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string ToName { get; set; }
    public required string Link { get; set; }
}