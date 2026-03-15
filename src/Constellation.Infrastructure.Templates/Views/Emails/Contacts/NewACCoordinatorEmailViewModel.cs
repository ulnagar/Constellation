namespace Constellation.Infrastructure.Templates.Views.Emails.Contacts;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;

public sealed class NewACCoordinatorEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Contacts/NewACCordinatorEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public readonly string Link = $"{BaseUrl}";
    public required string PartnerSchool { get; set; }

    public required EmailRecipient InstructionalLeader { get; set; }
}