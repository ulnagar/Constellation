namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Core.Models.Stocktake;
using Core.Models.WorkFlow.Identifiers;
using Shared;

public sealed class ActionAssignedEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/WorkFlow/ActionAssignedEmail.cshtml";

    public string TeacherName { get; set; }
    public CaseId CaseId { get; set; }
    public string CaseDescription { get; set; }
    public ActionId ActionId { get; set; }
    public string ActionDescription { get; set; }

    public string Link { get; set; }
}
