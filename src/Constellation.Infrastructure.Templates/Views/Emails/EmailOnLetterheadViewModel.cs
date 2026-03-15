namespace Constellation.Infrastructure.Templates.Views.Emails;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class EmailOnLetterheadViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/EmailOnLetterhead.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string Body { get; set; }
}
