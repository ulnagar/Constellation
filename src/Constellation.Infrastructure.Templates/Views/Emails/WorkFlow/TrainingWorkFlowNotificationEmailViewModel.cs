namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Shared;
using System;

public sealed class TrainingWorkFlowNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/WorkFlow/TrainingWorkFlowNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StaffName { get; set; }
    public required string ModuleName { get; set; }
    public required DateOnly DueDate { get; set; }
    public required int DaysUntilDue { get; set; }
}
