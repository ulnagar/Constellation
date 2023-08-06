namespace Constellation.Application.DTOs.EmailRequests;

using System.Collections.Generic;

public class LessonMissedNotificationEmail : EmailBaseClass
{
    public string SchoolName { get; set; }
    public NotificationSequence NotificationType { get; set; }
    public List<LessonEmail.LessonItem> Lessons { get; set; } = new();

    public enum NotificationSequence
    {
        First,
        Second,
        Third,
        Final,
        Alert
    }
}
