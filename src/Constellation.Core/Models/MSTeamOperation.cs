namespace Constellation.Core.Models;

using Constellation.Core.Enums;

public abstract class MSTeamOperation
{
    public int Id { get; set; }
    public MSTeamOperationAction Action { get; set; }
    public MSTeamOperationPermissionLevel PermissionLevel { get; set; }
    public DateTime DateScheduled { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}

public abstract class OfferingMSTeamOperation : MSTeamOperation
{
    public int OfferingId { get; set; }
    public virtual CourseOffering? Offering { get; set; }
}

public class StudentMSTeamOperation : OfferingMSTeamOperation
{
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
}

public class TeacherMSTeamOperation : OfferingMSTeamOperation
{
    public string StaffId { get; set; } = string.Empty;
    public virtual Staff? Staff { get; set; }
    public int? CoverId { get; set; }
    public virtual TeacherClassCover? Cover { get; set; }
}

public class CasualMSTeamOperation : OfferingMSTeamOperation
{
    public int CasualId { get; set; }
    public virtual Casual? Casual { get; set; }
    public int CoverId { get; set; }
    public virtual CasualClassCover? Cover { get; set; }

}

public class GroupMSTeamOperation : OfferingMSTeamOperation
{
    public Guid FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }
}

public abstract class EventMSTeamOperation : MSTeamOperation
{
    public string TeamName { get; set; } = string.Empty;
}

public class StudentEnrolledMSTeamOperation : EventMSTeamOperation
{
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
}

public class TeacherEmployedMSTeamOperation : EventMSTeamOperation
{
    public string StaffId { get; set; } = string.Empty;
    public virtual Staff? Staff { get; set; }
}

public class ContactAddedMSTeamOperation : EventMSTeamOperation
{
    public int ContactId { get; set; }
    public virtual SchoolContact? Contact { get; set; }
}