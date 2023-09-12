using Constellation.Core.Enums;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

namespace Constellation.Core.Models
{
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
    }

    public abstract class OfferingMSTeamOperation : MSTeamOperation
    {
        public OfferingId OfferingId { get; set; }
        public Offering Offering { get; set; }
    }

    public class StudentMSTeamOperation : OfferingMSTeamOperation
    {
        public string StudentId { get; set; }
        public Student Student { get; set; }
    }

    public class TeacherMSTeamOperation : OfferingMSTeamOperation
    {
        public string StaffId { get; set; }
        public Staff Staff { get; set; }
    }

    public class CasualMSTeamOperation : OfferingMSTeamOperation
    {
        public Guid CasualId { get; set; }
    }

    public class GroupMSTeamOperation : OfferingMSTeamOperation
    {
        public Guid FacultyId { get; set; }
        public Faculty Faculty { get; set; }
    }

    public abstract class EventMSTeamOperation : MSTeamOperation
    {
        public string TeamName { get; set; }
    }

    public class StudentEnrolledMSTeamOperation : EventMSTeamOperation
    {
        public string StudentId { get; set; }
        public Student Student { get; set; }
    }

    public class TeacherEmployedMSTeamOperation : EventMSTeamOperation
    {
        public string StaffId { get; set; }
        public Staff Staff { get; set; }
    }

    public class ContactAddedMSTeamOperation : EventMSTeamOperation
    {
        public int ContactId { get; set; }
        public SchoolContact Contact { get; set; }
    }

    public class GroupTutorialCreatedMSTeamOperation : EventMSTeamOperation
    {
        public GroupTutorialId TutorialId { get; set; }
        public GroupTutorial GroupTutorial { get; set; }
    }

    public sealed class TeacherAssignmentMSTeamOperation : EventMSTeamOperation
    {
        public string StaffId { get; set; }
    }

    public sealed class StudentOfferingMSTeamOperation : EventMSTeamOperation
    {
        public string StudentId { get; set; }
    }
}