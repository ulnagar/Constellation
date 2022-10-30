using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class Staff
    {
        public Staff()
        {
            CourseSessions = new List<OfferingSession>();
            ClassCovers = new List<TeacherClassCover>();
            AdobeConnectOperations = new List<TeacherAdobeConnectOperation>();
            AdobeConnectGroupOperations = new List<TeacherAdobeConnectGroupOperation>();
            MSTeamOperations = new List<TeacherMSTeamOperation>();
            ResponsibleCourses = new List<Course>();
            ClassworkNotifications = new List<ClassworkNotification>();

            IsDeleted = false;
            DateEntered = DateTime.Now;
        }

        public string StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PortalUsername { get; set; }
        public string AdobeConnectPrincipalId { get; set; }
        public Faculty Faculty { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateEntered { get; set; }
        public string SchoolCode { get; set; }
        public virtual School School { get; set; }
        public string DisplayName => FirstName + " " + LastName;
        public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
        public ICollection<OfferingSession> CourseSessions { get; set; }
        public ICollection<TeacherClassCover> ClassCovers { get; set; }
        public ICollection<TeacherAdobeConnectOperation> AdobeConnectOperations { get; set; }
        public ICollection<TeacherAdobeConnectGroupOperation> AdobeConnectGroupOperations { get; set; }
        public ICollection<TeacherMSTeamOperation> MSTeamOperations { get; set; }
        public ICollection<Course> ResponsibleCourses { get; set; }
        public ICollection<ClassworkNotification> ClassworkNotifications { get; set; }
        public List<TrainingCompletion> TrainingCompletionRecords { get; set; } = new();
    }
}