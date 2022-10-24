using Constellation.Infrastructure.Templates.Views.Shared;

namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons
{
    public class StudentMarkedPresentEmailViewModel : EmailLayoutBaseViewModel
    {
        public string StudentName { get; set; }
        public string Subject { get; set; }
        public string LessonTitle { get; set; }
    }
}
