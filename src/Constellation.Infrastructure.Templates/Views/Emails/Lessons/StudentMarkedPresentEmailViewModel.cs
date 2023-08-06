namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons;

using Constellation.Infrastructure.Templates.Views.Shared;

public class StudentMarkedPresentEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string Subject { get; set; }
    public string LessonTitle { get; set; }
}
