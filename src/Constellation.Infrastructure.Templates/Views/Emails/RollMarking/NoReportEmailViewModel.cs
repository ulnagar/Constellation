namespace Constellation.Infrastructure.Templates.Views.Emails.RollMarking;

using Shared;

public sealed class NoReportEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/RollMarking/NoReportEmail.cshtml";
    public override string ViewLocation => _viewLocation;
}