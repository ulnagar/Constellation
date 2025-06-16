namespace Constellation.Core.Models;

using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Faculties;
using GroupTutorials;
using Identifiers;
using Offerings;
using SchoolContacts;
using SchoolContacts.Identifiers;
using StaffMembers;
using StaffMembers.Identifiers;
using Students;
using System;

public abstract class MSTeamOperation
{
    public int Id { get; set; }
    public MSTeamOperationAction Action { get; set; }
    public MSTeamOperationPermissionLevel PermissionLevel { get; set; }
    public DateTime DateScheduled { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CoverId { get; set; }


    public void Complete()
    {
        IsCompleted = true;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public bool IsOutstanding()
    {
        // Has it been marked Deleted or Completed?
        return DateScheduled <= DateTime.Today && !(IsDeleted || IsCompleted);
    }
}

public abstract class OfferingMSTeamOperation : MSTeamOperation
{
    public OfferingId OfferingId { get; set; }
    public Offering Offering { get; set; }
}

public class StudentMSTeamOperation : OfferingMSTeamOperation
{
    public StudentId StudentId { get; set; }
    public Student Student { get; set; }
}

public class TeacherMSTeamOperation : OfferingMSTeamOperation
{
    public StaffId StaffId { get; set; }
    public StaffMember Staff { get; set; }
}

public class CasualMSTeamOperation : OfferingMSTeamOperation
{
    public Guid CasualId { get; set; }
}

public class GroupMSTeamOperation : OfferingMSTeamOperation
{
    public FacultyId FacultyId { get; set; }
    public Faculty Faculty { get; set; }
}

public abstract class EventMSTeamOperation : MSTeamOperation
{
    public string TeamName { get; set; }
}

public class StudentEnrolledMSTeamOperation : EventMSTeamOperation
{
    public StudentId StudentId { get; set; }
    public Student Student { get; set; }
}

public class TeacherEmployedMSTeamOperation : EventMSTeamOperation
{
    public StaffId StaffId { get; set; }
    public StaffMember Staff { get; set; }
}

public class ContactAddedMSTeamOperation : EventMSTeamOperation
{
    public SchoolContactId ContactId { get; set; }
    public SchoolContact Contact { get; set; }
}

public class GroupTutorialCreatedMSTeamOperation : EventMSTeamOperation
{
    public GroupTutorialId TutorialId { get; set; }
    public GroupTutorial GroupTutorial { get; set; }
}

public sealed class TeacherAssignmentMSTeamOperation : EventMSTeamOperation
{
    public StaffId StaffId { get; set; }
}

public sealed class StudentOfferingMSTeamOperation : EventMSTeamOperation
{
    public StudentId StudentId { get; set; }
}