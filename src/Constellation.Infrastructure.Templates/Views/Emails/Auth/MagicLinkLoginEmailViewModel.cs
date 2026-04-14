namespace Constellation.Infrastructure.Templates.Views.Emails.Auth;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class MagicLinkLoginEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Auth/MagicLinkLoginEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public MagicLinkLoginEmailViewModel(
        string name,
        string link)
    {
        ToName = name;
        Link = link;
        //Link = Uri.EscapeDataString(link);
    }

    public string ToName { get; private set; }
    public string Link { get; private set; }
}