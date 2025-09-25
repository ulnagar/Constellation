namespace Constellation.Application.DTOs;

using Constellation.Core.Models;
using System.Collections.Generic;

public sealed class MSTeamOperationsList
{
    public List<StudentMSTeamOperation> StudentOperations { get; set; } = [];
    public List<TeacherMSTeamOperation> TeacherOperations { get; set; } = [];
    public List<CasualMSTeamOperation> CasualOperations { get; set; } = [];
    public List<GroupMSTeamOperation> GroupOperations { get; set; } = [];
    public List<StudentEnrolledMSTeamOperation> EnrolmentOperations { get; set; } = [];
    public List<TeacherEmployedMSTeamOperation> EmploymentOperations { get; set; } = [];
    public List<ContactAddedMSTeamOperation> ContactOperations { get; set; } = [];
    public List<GroupTutorialCreatedMSTeamOperation> GroupTutorialOperations { get; set; } = [];
    public List<TeacherAssignmentMSTeamOperation> AssignmentOperations { get; set; } = [];
    public List<StudentOfferingMSTeamOperation> StudentOfferingOperations { get; set; } = [];
    public List<TutorialCreatedMSTeamOperation> TutorialOperations { get; set; } = [];
}