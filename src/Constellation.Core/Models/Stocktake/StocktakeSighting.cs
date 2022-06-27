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
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string SightedBy { get; set; }
        public DateTime SightedAt { get; set; }

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
