namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class CaseActionsCompletedEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/WorkFlow/CaseActionsCompletedEmail.cshtml";

    public string CaseDescription { get; set; }
    public string Link { get; set; }

}
