namespace Constellation.Infrastructure.Templates.Views.Emails.Contacts;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;

public sealed class NewSciencePracTeacherEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Contacts/NewSciencePracTeacherEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public readonly string Link = $"{BaseUrl}";

    public required string PartnerSchool { get; set; }
    public required EmailRecipient Coordinator { get; set; }
    public required EmailRecipient HeadTeacher { get; set; }
}