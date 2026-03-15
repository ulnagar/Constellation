namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class CaseActionsCompletedEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/WorkFlow/CaseActionsCompletedEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public string Link => $"{BaseUrl}{LinkPart}";
    
    public required string CaseDescription { get; set; }
    public required string LinkPart { get; set; }

}
