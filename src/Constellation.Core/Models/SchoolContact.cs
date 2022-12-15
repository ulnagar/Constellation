namespace Constellation.Core.Models;

public class SchoolContact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime DateEntered { get; set; } = DateTime.Now;
    public bool SelfRegistered { get; set; }
    public string DisplayName => FirstName + " " + LastName;
    public List<SchoolContactRole> Assignments { get; set; } = new();
    public List<LessonRoll> LessonRolls { get; set; } = new();
}
