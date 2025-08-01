namespace Constellation.Infrastructure.Templates.Views.Emails;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class EmailOnLetterheadViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/EmailOnLetterhead.cshtml";

    public string Body { get; set; }
}
