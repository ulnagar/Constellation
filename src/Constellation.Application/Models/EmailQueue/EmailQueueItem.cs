using System;

namespace Constellation.Application.Models.EmailQueue;

public class EmailQueueItem
{
    public static class EmailQueueReferenceType
    {
        public static class Absences
        {
            public static class Students
            {
                public const string Notification = "Absence-Student-Notification";
                public const string Reminder = "Absence-Student-Reminder";
            }

            public static class Parents
            {
                public const string Notification = "Absence-Parent-Notification";
                public const string Reminder = "Absence-Parent-Reminder";
            }

            public static class Coordinators
            {
                public const string Notification = "Absence-Coordinator-Notification";
                public const string Reminder = "Absence-Coordinator-Reminder";
            }

            public static class Administration
            {
                public const string Response = "Absence-Administration-Response";
            }
        }

        public static class Covers
        {
            public static class Casuals
            {
                public static class SingleDay
                {
                    public const string Appointment = "Cover-Casual-SingleDay-Appointment";
                    public const string Modification = "Cover-Casual-SingleDay-Modification";
                    public const string Cancellation = "Cover-Casual-SingleDay-Cancellation";
                }

                public static class MultiDay
                {
                    public const string Notification = "Cover-Casual-MultiDay-Notification";
                    public const string Modifiation = "Cover-Casual-MultiDay-Modification";
                    public const string Cancellation = "Cover-Casual-MutliDay-Cancellation";
                }
            }

            public static class Teachers
            {
                public static class SingleDay
                {
                    public const string Appointment = "Cover-Teacher-SingleDay-Appointment";
                    public const string Modification = "Cover-Teacher-SingleDay-Modification";
                    public const string Cancellation = "Cover-Teacher-SingleDay-Cancellation";
                }

                public static class MultiDay
                {
                    public const string Notification = "Cover-Teacher-MultiDay-Notification";
                    public const string Modifiation = "Cover-Teacher-MultiDay-Modification";
                    public const string Cancellation = "Cover-Teacher-MutliDay-Cancellation";
                }
            }
        }

        public static class AttendanceReport
        {
            public const string Parent = "AttendanceReport-Parent";
            public const string School = "AttendanceReport-School";
        }

        public static class AdminNotifications
        {
            public static class ParentDetails
            {
                public const string Contact = "AdminNotifications-ParentDetails-Contact";
            }

            public static class StudentDetails
            {
                public const string SentralEntry = "AdminNotifications-StudentDetails-SentralEntry";
            }

            public static class SystemAlerts
            {
                public const string SMSLowCreditBalance = "AdminNotifications-SystemAlerts-SMSLowCreditBalance";
                public const string ServiceLog = "AdminNotifications-SystemAlerts-ServiceLog";
                public const string RollMarkingReport = "AdminNotifications-SystemAlerts-RollMarkingReport";
            }
        }

        public static class LessonRolls
        {
            public static class Schools
            {
                public static class MissingRoll
                {
                    public const string FirstWarning = "LessonRolls-Schools-MissingRoll-FirstWarning";
                    public const string SecondWarning = "LessonRolls-Schools-MissingRoll-SecondWarning";
                    public const string ThirdWarning = "LessonRolls-Schools-MissingRoll-ThirdWarning";
                    public const string FinalWarning = "LessonRolls-Schools-MissingRoll-FinalWarning";
                }
            }

            public static class Administration
            {
                public const string MissingRoll = "LessonRolls-Administration-MissingRoll";
            }
        }

        public static class MissedClasswork
        {
            public static class Requests
            {
                public const string Teacher = "MissedClasswork-Requests-Teacher";
            }

            public static class Notifications
            {
                public const string Student = "MissedClasswork-Notifications-Student";
                public const string Parent = "MissedClasswork-Notification-Parent";
                public const string Teacher = "MissedClasswork-Notification-Teacher";
            }
        }
    }

    public Guid Id { get; set; }
    public string ReferenceId { get; set; }
    public string ReferenceType { get; set; }
}
