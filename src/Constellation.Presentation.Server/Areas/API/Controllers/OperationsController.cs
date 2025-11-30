namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Domains.LinkedSystems.Teams.Models;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.LinkedSystems;
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
using System.Threading;

[ApiController]
public sealed class OperationsController : ControllerBase
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
    [HttpGet("api/v1/operations/due")]
    public async Task<IEnumerable<TeamsOperationDto>> GetDue()
    {
        MSTeamOperationsList operations = await _operationsRepository.ToProcess();

        List<TeamOperation> newOperations = await _teamOperationRepository.GetDue();

        return await BuildOperations(operations, newOperations);
    }


    // GET api/Operations/Overdue
    [HttpGet("api/v1/Operations/Overdue")]
    public async Task<IEnumerable<TeamsOperationDto>> GetOverdue()
    {
        MSTeamOperationsList operations = await _operationsRepository.OverdueToProcess();

        List<TeamOperation> newOperations = await _teamOperationRepository.GetOverdue();

        return await BuildOperations(operations, newOperations);
    }

    private async Task<ICollection<TeamsOperationDto>> BuildOperations(
        MSTeamOperationsList operations,
        List<TeamOperation> newOperations)
    {
        List<TeamsOperationDto> returnData = new();

        foreach (TeacherAssignmentMSTeamOperation operation in operations.AssignmentOperations)
        {
            StaffMember staffMember = await _staffRepository.GetById(operation.StaffId);

            if (staffMember is null)
                continue;

            TeamsOperationDto teamOperation = new()
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

            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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

            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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

            TeamsOperationDto teamOperation = new()
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
            TeamsOperationDto teamOperation = new()
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
        
        foreach (TeamOperation operation in newOperations)
        {
            TeamsOperationDto dto = operation switch
            {
                CreateTeamTeamOperation createTeam => new TeamsOperationDto()
                {
                    Id = createTeam.Id,
                    TeamName = createTeam.Name,
                    Action = "Group",
                    AdditionalInformation = createTeam.Description
                },
                _ => throw new ArgumentOutOfRangeException()
            };

            returnData.Add(dto);
        }

        return returnData;
    }

    // POST api/Operations/Complete
    [HttpPost("api/v1/Operations/Complete/{id}")]
    public async Task Complete(int id)
    {
        MSTeamOperation operation = await _operationsRepository.ForMarkingCompleteOrCancelled(id);

        if (operation != null)
        {
            operation.Complete();
            await _unitOfWork.CompleteAsync();

            return;
        }

        TeamOperation newOperation = await _teamOperationRepository.GetById(id);

        if (newOperation != null)
        {
            newOperation.Complete();
            await _unitOfWork.CompleteAsync();

            return;
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
    [HttpGet("api/v2/Operations/Due")]
    public async Task<IEnumerable<TeamsOperationDto>> GetDueV2(
        CancellationToken cancellationToken = default)
    {
        List<TeamOperation> operations = await _teamOperationRepository.GetDue(cancellationToken);

        List<TeamsOperationDto> response = [];

        foreach (var operation in operations)
        {
            TeamsOperationDto dto = operation switch
            {
                CreateTeamTeamOperation createTeam => new TeamsOperationDto()
                {
                    Id = createTeam.Id,
                    TeamName = createTeam.Name,
                    Action = "Group",
                    AdditionalInformation = createTeam.Description
                },
                CreateTeamChannelTeamOperation createChannel => new TeamsOperationDto()
                {

                },
                ModifyTeamMembershipTeamOperation modifyMembership => new TeamsOperationDto()
                {

                },
                ModifyTeamChannelMembershipTeamOperation modifyChannelMembership => new TeamsOperationDto()
                {

                },
                ArchiveTeamTeamOperation archiveTeam => new TeamsOperationDto()
                {

                },
                ArchiveTeamChannelTeamOperation archiveChannel => new TeamsOperationDto()
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