using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public abstract class ClassCover
    {
        public ClassCover()
        {
            IsDeleted = false;

            AdobeConnectOperations = new List<AdobeConnectOperation>();
            MSTeamOperations = new List<MSTeamOperation>();
            ClassworkNotifications = new List<ClassworkNotification>();
        }

        public int Id { get; set; }
        public CourseOffering Offering { get; set; }
        public int OfferingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<AdobeConnectOperation> AdobeConnectOperations { get; set; }
        public ICollection<MSTeamOperation> MSTeamOperations { get; set; }

        public ICollection<ClassworkNotification> ClassworkNotifications { get; set; }
        public bool IsCurrent => (StartDate <= DateTime.Now && EndDate >= DateTime.Now);
        public bool IsFuture => (StartDate > DateTime.Now);
               
        public void Delete()
        {
            IsDeleted = true;
        }
    }

    public class CasualClassCover : ClassCover
    {
        public CasualClassCover() { }

        public CasualClassCover(int offeringId, int casualId, DateTime startDate, DateTime endDate)
        {
            CasualId = casualId;
            OfferingId = offeringId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public Casual Casual { get; set; }
        public int CasualId { get; set; }
    }

    public class TeacherClassCover : ClassCover
    {
        public TeacherClassCover() { }

        public Staff Staff { get; set; }
        public string StaffId { get; set; }
    }
}