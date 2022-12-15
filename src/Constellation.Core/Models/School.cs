namespace Constellation.Core.Models;

public class School
{
    public School()
    {
        StaffAssignments = new List<SchoolContactRole>();
        Students = new List<Student>();
        Staff = new List<Staff>();
    }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FaxNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public bool HeatSchool { get; set; }
    public string Electorate { get; set; } = string.Empty;
    public string PrincipalNetwork { get; set; } = string.Empty;
    public string TimetableApplication { get; set; } = string.Empty;
    public string RollCallGroup { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Website { get; set; } = string.Empty;
    public bool HasStudents => (Students.Any(students => !students.IsDeleted));
    public bool HasStaff => (Staff.Any(staff => !staff.IsDeleted));
    public virtual ICollection<SchoolContactRole> StaffAssignments { get; set; }
    public virtual ICollection<Student> Students { get; set; }
    public virtual ICollection<Staff> Staff { get; set; }
}
