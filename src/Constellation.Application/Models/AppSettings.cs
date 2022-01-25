using System;

namespace Constellation.Application.Models
{
    public class AppSettings
    {
        public AppSettings()
        {
            Created = DateTime.Now;

            Absences = new AbsencesModule();
        }

        public int Id { get; set; }
        public DateTime Created { get; set; }
        public AbsencesModule Absences { get; set; }

        public string AdobeConnectDefaultFolder { get; set; }
        public string SentralContactPreference { get; set; }

        public string LessonsCoordinatorEmail { get; set; }
        public string LessonsCoordinatorName { get; set; }
        public string LessonsCoordinatorTitle { get; set; }
        public string LessonsHeadTeacherEmail { get; set; }

        public class AbsencesModule
        {
            /// <summary>
            /// Collection of reasons that Whole Absences can have in external systems that are not considered absences.
            /// </summary>
            public string DiscountedWholeReasons { get; set; }

            /// <summary>
            /// Collection of reasons that Partial Absences can have in external systems that are not considered absences.
            /// </summary>
            public string DiscountedPartialReasons { get; set; }

            /// <summary>
            /// Date from which absences are considered for the notification system.
            /// </summary>
            public DateTime AbsenceScanStartDate { get; set; }
            
            /// <summary>
            /// Partial absences shorter than this many minutes will be ignored.
            /// </summary>
            public int PartialLengthThreshold { get; set; }

            /// <summary>
            /// The email address that absence explanations should be sent to when received.
            /// </summary>
            public string ForwardingEmailAbsenceCoordinator { get; set; }

            /// <summary>
            /// The email address that truancy notifications should be sent to when generated.
            /// </summary>
            public string ForwardingEmailTruancyOfficer { get; set; }

            /// <summary>
            /// The name used in the external notification email signatures.
            /// </summary>
            public string AbsenceCoordinatorName { get; set; }

            /// <summary>
            /// The title used in the external notification email signatures.
            /// </summary>
            public string AbsenceCoordinatorTitle { get; set; }

            /// <summary>
            /// The email address used to send external notification emails from.
            /// </summary>
            public string AbsenceCoordinatorEmail { get; set; }
        }
    }
}
