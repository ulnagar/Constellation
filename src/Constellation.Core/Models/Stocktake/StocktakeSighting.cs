namespace Constellation.Core.Models.Stocktake;

public class StocktakeSighting
{
    public Guid Id { get; set; }
    public Guid StocktakeEventId { get; set; }
    public virtual StocktakeEvent? StocktakeEvent { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string AssetNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationCategory { get; set; } = string.Empty; 
    public string LocationName { get; set; } = string.Empty;
    // If the LocationCategory is LocationCategories.PublicSchool, populate with school code for lookup
    public string LocationCode { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    // If the UserType is UserTypes.Student, populate with the student id for lookup
    // If the UserType is UserTypes.Staff, populate with the staff id for lookup
    // If the UserType is UserTypes.School, populate with the school code for lookup
    public string UserCode { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string SightedBy { get; set; } = string.Empty;
    public DateTime SightedAt { get; set; }

    // If the sighting was incorrect for some reason, allow the user to cancel it if they provide a reason
    public bool IsCancelled { get; set; }
    public string CancellationComment { get; set; } = string.Empty;
    public string CancelledBy { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; } = new DateTime();

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
