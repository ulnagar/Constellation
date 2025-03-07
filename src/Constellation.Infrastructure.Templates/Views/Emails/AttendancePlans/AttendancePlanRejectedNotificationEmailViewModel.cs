namespace Constellation.Infrastructure.Templates.Views.Emails.AttendancePlans;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class AttendancePlanRejectedNotificationEmailViewModel
    : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AttendancePlans/AttendancePlanRejectedNotificationEmail.cshtml";

    public string Student { get; set; }
    public string Grade { get; set; }
    public string Comment { get; set; }
}