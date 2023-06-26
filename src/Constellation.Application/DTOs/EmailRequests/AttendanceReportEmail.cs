namespace Constellation.Application.DTOs.EmailRequests;

using System;
using System.Collections.Generic;
using System.Net.Mail;

public class AttendanceReportEmail : EmailBaseClass
{
    public List<Attachment> Attachments { get; set; } = new();
    public string StudentName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public NotificationSequence NotificationType { get; set; }

    public enum NotificationSequence
    {
        Student,
        School
    }
}
