namespace Constellation.Infrastructure.Templates.Views.Emails.Auth;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;

public sealed class EnrolmentPortalEmailLoginEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Auth/EnrolmentPortalEmailLoginEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public EnrolmentPortalEmailLoginEmailViewModel()
    { }

    public required Name Name { get; set; }
    public required string Link { get; set; }
}