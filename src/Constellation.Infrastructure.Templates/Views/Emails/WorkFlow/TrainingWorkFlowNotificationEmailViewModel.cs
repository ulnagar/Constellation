namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Shared;
using System;

public sealed class TrainingWorkFlowNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/WorkFlow/TrainingWorkFlowNotificationEmail.cshtml";

    public string StaffName { get; set; }
    public string ModuleName { get; set; }
    public DateOnly DueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public string Reviewer { get; set; }
}
