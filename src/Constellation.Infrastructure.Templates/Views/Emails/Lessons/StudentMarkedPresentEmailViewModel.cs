namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons;

using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class StudentMarkedPresentEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Lessons/StudentMarkedPresentEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StudentName { get; set; }
    public required string Subject { get; set; }
    public required string LessonTitle { get; set; }
}
