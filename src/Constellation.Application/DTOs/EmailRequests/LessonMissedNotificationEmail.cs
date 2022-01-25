using System.Collections.Generic;

namespace Constellation.Application.DTOs.EmailRequests
{
    public class LessonMissedNotificationEmail : EmailBaseClass
    {
        public LessonMissedNotificationEmail()
        {
            Lessons = new List<EmailDtos.LessonEmail.LessonItem>();
        }

        public string SchoolName { get; set; }
        public NotificationSequence NotificationType { get; set; }
        public ICollection<EmailDtos.LessonEmail.LessonItem> Lessons { get; set; }

        public enum NotificationSequence
        {
            First,
            Second,
            Third,
            Final,
            Alert
        }
    }
}
