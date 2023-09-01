namespace Constellation.Application.DTOs;

using Constellation.Core.Models;
using System.Collections.Generic;

public sealed class MSTeamOperationsList
{
    public List<StudentMSTeamOperation> StudentOperations { get; set; } = new();
    public List<TeacherMSTeamOperation> TeacherOperations { get; set; } = new();
    public List<CasualMSTeamOperation> CasualOperations { get; set; } = new();
    public List<GroupMSTeamOperation> GroupOperations { get; set; } = new();
    public List<StudentEnrolledMSTeamOperation> EnrolmentOperations { get; set; } = new();
    public List<TeacherEmployedMSTeamOperation> EmploymentOperations { get; set; } = new();
    public List<ContactAddedMSTeamOperation> ContactOperations { get; set; } = new();
    public List<GroupTutorialCreatedMSTeamOperation> TutorialOperations { get; set; } = new();
    public List<TeacherAssignmentMSTeamOperation> AssignmentOperations { get; set; } = new();
    public List<StudentOfferingMSTeamOperation> StudentOfferingOperations { get; set; } = new();
}