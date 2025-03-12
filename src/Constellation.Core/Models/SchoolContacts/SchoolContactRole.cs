namespace Constellation.Core.Models.SchoolContacts;

using Enums;
using Identifiers;
using Primitives;
using System;

public sealed class SchoolContactRole : IAuditableEntity
{
    //public const string SciencePrac = "Science Practical Teacher";
    //public const string Coordinator = "Aurora College Coordinator";
    //public const string Principal = "Principal";
    //public const string Timetabler = "Timetable Officer";
    //public const string NESACoordinator = "NESA Coordinator";
    //public const string TechnologyOfficer = "Technology Support Officer";

    private SchoolContactRole() { } // Required for EF Core

    internal SchoolContactRole(
        SchoolContactId contactId,
        Position role,
        string schoolCode, 
        string schoolName,
        string note)
    {
        Id = new();
        SchoolContactId = contactId;
        Role = role;
        SchoolCode = schoolCode;
        SchoolName = schoolName;
        Note = note;
    }

    public SchoolContactRoleId Id { get; private set; }
    public SchoolContactId SchoolContactId { get; private set; }
    public Position Role { get; private set; }
    public string SchoolCode { get; private set; }
    public string SchoolName { get; private set; }
    public string Note { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal void Delete() => IsDeleted = true;

    internal void Update(string note) => Note = note;
}