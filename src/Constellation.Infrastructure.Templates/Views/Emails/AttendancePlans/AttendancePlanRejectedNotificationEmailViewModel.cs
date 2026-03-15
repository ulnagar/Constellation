namespace Constellation.Infrastructure.Templates.Views.Emails.AttendancePlans;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class AttendancePlanRejectedNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AttendancePlans/AttendancePlanRejectedNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string Student { get; set; }
    public required string Grade { get; set; }
    public required string Comment { get; set; }
}