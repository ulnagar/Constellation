using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Application.DTOs
{ 
    public class MSTeamOperationsList
    {
        public ICollection<StudentMSTeamOperation> StudentOperations { get; set; }
        public ICollection<TeacherMSTeamOperation> TeacherOperations { get; set; }
        public ICollection<CasualMSTeamOperation> CasualOperations { get; set; }
        public ICollection<GroupMSTeamOperation> GroupOperations { get; set; }
        public ICollection<StudentEnrolledMSTeamOperation> EnrolmentOperations { get; set; }
        public ICollection<TeacherEmployedMSTeamOperation> EmploymentOperations { get; set; }
        public ICollection<ContactAddedMSTeamOperation> ContactOperations { get; set; }
        public ICollection<GroupTutorialCreatedMSTeamOperation> TutorialOperations { get; set; }

        public MSTeamOperationsList()
        {
            StudentOperations = new List<StudentMSTeamOperation>();
            TeacherOperations = new List<TeacherMSTeamOperation>();
            CasualOperations = new List<CasualMSTeamOperation>();
            GroupOperations = new List<GroupMSTeamOperation>();
            EnrolmentOperations = new List<StudentEnrolledMSTeamOperation>();
            EmploymentOperations = new List<TeacherEmployedMSTeamOperation>();
            ContactOperations = new List<ContactAddedMSTeamOperation>();
            TutorialOperations = new List<GroupTutorialCreatedMSTeamOperation>();
        }
    }
}