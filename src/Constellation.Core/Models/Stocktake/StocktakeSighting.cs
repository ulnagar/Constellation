using System;

namespace Constellation.Core.Models.Stocktake
{
    public class StocktakeSighting
    {
        public Guid Id { get; set; }
        public Guid StocktakeEventId { get; set; }
        public virtual StocktakeEvent StocktakeEvent { get; set; }
        public string SerialNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string LocationCategory { get; set; }
        public string LocationName { get; set; }
        // If the LocationCategory is LocationCategories.PublicSchool, populate with school code for lookup
        public string LocationCode { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        // If the UserType is UserTypes.Student, populate with the student id for lookup
        // If the UserType is UserTypes.Staff, populate with the staff id for lookup
        // If the UserType is UserTypes.School, populate with the school code for lookup
        public string UserCode { get; set; }
        public string Comment { get; set; }
        public string SightedBy { get; set; }
        public DateTime SightedAt { get; set; }

        // If the sighting was incorrect for some reason, allow the user to cancel it if they provide a reason
        public bool IsCancelled { get; set; }
        public string CancellationComment { get; set; }
        public string CancelledBy { get; set; }
        public DateTime CancelledAt { get; set; }

        public class UserTypes
        {
            public const string Student = "Student";
            public const string Staff = "Staff Member";
            public const string School = "Partner School";
            public const string CommunityMember = "Community Member";
            public const string Other = "Other";
        }

        public class LocationCategories
        {
            public const string AuroraCollege = "Aurora College";
            public const string PublicSchool = "Public School";
            public const string StateOffice = "State Office";
            public const string PrivateResidence = "Private Residence";
            public const string Other = "Other";
        }
    }
}
