namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System.Collections.Generic;

public class School : BaseAuditableEntity
{
    public string SchoolCode { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Town { get; set; }
    public string State { get; set; }
    public string PostCode { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public bool HeatSchool { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Website { get; set; }

    public IList<Student> Students { get; private set; } = new List<Student>();
    public IList<StaffMember> StaffMembers { get; private set; } = new List<StaffMember>();
}