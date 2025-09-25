namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Domains.LinkedSystems.Teams.Models;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Identifiers;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.Casuals;
using Core.Models.Operations;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Microsoft.AspNetCore.Mvc;
using Models;

[ApiController]
public class OperationsController : ControllerBase
{
    private readonly ITeamOperationRepository _teamOperationRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OperationsController(
        ITeamOperationRepository teamOperationRepository,
        IMSTeamOperationsRepository operationsRepository,
        ITeamRepository teamRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _teamOperationRepository = teamOperationRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    #region v1 Operations

    // GET api/Operations/Due
    [Route("api/v1/Operations/Due")]
    public async Task<IEnumerable<TeamsOperation>> GetDue()
    {
        MSTeamOperationsList operations = await _operationsRepository.ToProcess();

        return await BuildOperations(operations);
    }


    // GET api/Operations/Overdue
    [Route("api/v1/Operations/Overdue")]
    public async Task<IEnumerable<TeamsOperation>> GetOverdue()
    {
        MSTeamOperationsList operations = await _operationsRepository.OverdueToProcess();

        return await BuildOperations(operations);
    }

    private async Task<ICollection<TeamsOperation>> BuildOperations(MSTeamOperationsList operations)
    {
        List<TeamsOperation> returnData = new();

        foreach (TeacherAssignmentMSTeamOperation operation in operations.AssignmentOperations)
        {
            StaffMember staffMember = await _staffRepository.GetById(operation.StaffId);

            if (staffMember is null)
                continue;

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = staffMember.EmailAddress.Email
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            TeamResource? team = await GetTeam(operation.TeamName);

            if (team is null)
                continue;

            teamOperation.TeamId = team.Id.ToString();

            returnData.Add(teamOperation);
        }

        foreach (StudentOfferingMSTeamOperation operation in operations.StudentOfferingOperations)
        {
            Student student = await _studentRepository.GetById(operation.StudentId);
            
            if (student is null)
                continue;

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = student.EmailAddress.Email
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            TeamResource? team = await GetTeam(operation.TeamName);

            if (team is null)
                continue;

            teamOperation.TeamId = team.Id.ToString();

            returnData.Add(teamOperation);
        }

        foreach (StudentMSTeamOperation operation in operations.StudentOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = operation.Student.EmailAddress.Email
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            Guid? offeringTeamId = await _teamRepository.GetIdByOffering(operation.Offering.Name, operation.Offering.EndDate.Year.ToString());

            if (offeringTeamId is null)
                continue;

            teamOperation.TeamId = offeringTeamId.ToString();

            returnData.Add(teamOperation);
        }

        foreach (TeacherMSTeamOperation operation in operations.TeacherOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = operation.Staff.EmailAddress.Email,
                AdditionalInformation = "AllOwner"
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            Guid? offeringTeamId = await _teamRepository.GetIdByOffering(operation.Offering.Name, operation.Offering.EndDate.Year.ToString());

            if (offeringTeamId is null)
                continue;

            teamOperation.TeamId = offeringTeamId.ToString();

            returnData.Add(teamOperation);
        }

        foreach (CasualMSTeamOperation operation in operations.CasualOperations)
        {
            Casual casual = await _casualRepository.GetById(CasualId.FromValue(operation.CasualId));

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = casual.EmailAddress.Email,
                AdditionalInformation = "AllOwner"
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            Guid? offeringTeamId = await _teamRepository.GetIdByOffering(operation.Offering.Name, operation.Offering.EndDate.Year.ToString());

            if (offeringTeamId is null)
                continue;

            teamOperation.TeamId = offeringTeamId.ToString();

            returnData.Add(teamOperation);
        }

        foreach (GroupMSTeamOperation operation in operations.GroupOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                Action = "Group",
                Faculty = operation.Faculty.ToString()
            };

            Guid? offeringTeamId = await _teamRepository.GetIdByOffering(operation.Offering.Name, operation.Offering.EndDate.Year.ToString());

            if (offeringTeamId is null)
                continue;

            teamOperation.TeamId = offeringTeamId.ToString();

            returnData.Add(teamOperation);
        }

        foreach (TutorialCreatedMSTeamOperation operation in operations.TutorialOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                Action = "Group",
                AdditionalInformation = operation.TeamDescription
            };

            returnData.Add(teamOperation);
        }

        foreach (GroupTutorialCreatedMSTeamOperation operation in operations.GroupTutorialOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.GroupTutorial.EndDate:yyyy} - {operation.GroupTutorial.Name}",
                Action = "Group",
                AdditionalInformation = string.IsNullOrWhiteSpace(operation.TeamDescription) 
                    ? $"AC - {operation.GroupTutorial.EndDate:yyyy} - {operation.GroupTutorial.Name}"
                    : operation.TeamDescription
            };

            returnData.Add(teamOperation);
        }

        foreach (StudentEnrolledMSTeamOperation operation in operations.EnrolmentOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = operation.Student.EmailAddress.Email,
                AdditionalInformation = operation.Student.CurrentEnrolment?.Grade.ToString()
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            Guid? offeringTeamId = await _teamRepository.GetIdByOffering(operation.TeamName, operation.TeamName);

            if (offeringTeamId is null)
                continue;

            teamOperation.TeamId = offeringTeamId.ToString();
            
            returnData.Add(teamOperation);
        }

        foreach (TeacherEmployedMSTeamOperation operation in operations.EmploymentOperations)
        {
            StaffMember staffMember = await _staffRepository.GetById(operation.StaffId);

            if (staffMember is null)
                continue;

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = staffMember.EmailAddress.Email,
                AdditionalInformation = "All"
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            TeamResource? team = await GetTeam(operation.TeamName);

            if (team is null)
                continue;

            teamOperation.TeamId = team.Id.ToString();

            returnData.Add(teamOperation);
        }

        foreach (ContactAddedMSTeamOperation operation in operations.ContactOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = operation.Contact.EmailAddress
            };

            teamOperation.Action = operation.Action switch
            {
                MSTeamOperationAction.Add => "Add",
                MSTeamOperationAction.Remove => "Remove",
                MSTeamOperationAction.Promote => "Promote",
                MSTeamOperationAction.Demote => "Demote",
                _ => teamOperation.Action
            };

            teamOperation.Role = operation.PermissionLevel switch
            {
                MSTeamOperationPermissionLevel.Member => "Member",
                MSTeamOperationPermissionLevel.Owner => "Owner",
                _ => teamOperation.Role
            };

            TeamResource? team = await GetTeam(operation.TeamName);

            if (team is null)
                continue;

            teamOperation.TeamId = team.Id.ToString();

            returnData.Add(teamOperation);
        }

        return returnData;
    }

    // POST api/Operations/Complete
    [Route("api/v1/Operations/Complete/{id}")]
    [HttpPost]
    public async Task Complete(int id)
    {
        MSTeamOperation operation = await _operationsRepository.ForMarkingCompleteOrCancelled(id);

        if (operation != null)
        {
            operation.Complete();
            await _unitOfWork.CompleteAsync();
        }
    }

    private async Task<TeamResource?> GetTeam(string name)
    {
        List<Team> teams = await _teamRepository.GetByName(name);

        if (teams.Count == 0)
            return null;

        Team exactMatch = teams.FirstOrDefault(team => team.Name == name);

        if (exactMatch is not null)
        {
            return new(
                exactMatch.Id,
                exactMatch.Name,
                exactMatch.Description,
                exactMatch.Link,
                exactMatch.IsArchived);
        }

        return null;
    }

    #endregion

    #region v2 Operations
    [Route("api/v2/Operations/Due")]
    public async Task<IEnumerable<TeamsOperation>> GetDueV2(
        CancellationToken cancellationToken = default)
    {
        List<TeamOperation> operations = await _teamOperationRepository.GetDue(cancellationToken);

        List<TeamsOperation> response = [];

        foreach (var operation in operations)
        {
            TeamsOperation dto = operation switch
            {
                CreateTeamTeamOperation createTeam => new TeamsOperation()
                {
                    Id = createTeam.Id,
                    TeamName = createTeam.Name,
                    Action = "Group",
                    AdditionalInformation = createTeam.Description
                },
                CreateTeamChannelTeamOperation createChannel => new TeamsOperation()
                {

                },
                ModifyTeamMembershipTeamOperation modifyMembership => new TeamsOperation()
                {

                },
                ModifyTeamChannelMembershipTeamOperation modifyChannelMembership => new TeamsOperation()
                {

                },
                ArchiveTeamTeamOperation archiveTeam => new TeamsOperation()
                {

                },
                ArchiveTeamChannelTeamOperation archiveChannel => new TeamsOperation()
                {

                },
                _ => throw new ArgumentOutOfRangeException()
            };

            response.Add(dto);
        }

        return response;
    }

    #endregion
}