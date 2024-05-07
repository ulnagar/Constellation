namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.StaffMembers.GetStaffById;
using Application.Students.GetStudentById;
using Application.Students.Models;
using Application.Teams.GetTeamByName;
using Constellation.Application.Features.API.Operations.Queries;
using Constellation.Application.Teams.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.Casuals;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;

[Route("api/v1/Operations")]
[ApiController]
public class OperationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICasualRepository _casualRepository;

    public OperationsController(
        IUnitOfWork unitOfWork, 
        IMediator mediator, 
        ICasualRepository casualRepository)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _casualRepository = casualRepository;
    }

    // GET api/Operations/Due
    [Route("Due")]
    public async Task<IEnumerable<TeamsOperation>> GetDue()
    {
        MSTeamOperationsList operations = await _unitOfWork.MSTeamOperations.ToProcess();

        return await BuildOperations(operations);
    }


    // GET api/Operations/Overdue
    [Route("Overdue")]
    public async Task<IEnumerable<TeamsOperation>> GetOverdue()
    {
        MSTeamOperationsList operations = await _unitOfWork.MSTeamOperations.OverdueToProcess();

        return await BuildOperations(operations);
    }

    private async Task<ICollection<TeamsOperation>> BuildOperations(MSTeamOperationsList operations)
    {
        List<TeamsOperation> returnData = new();

        foreach (TeacherAssignmentMSTeamOperation operation in operations.AssignmentOperations)
        {
            Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(operation.StaffId));

            if (staffMember.IsFailure)
            {
                continue;
            }

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = staffMember.Value.EmailAddress.Email
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

            Result<TeamResource> request = await _mediator.Send(new GetTeamByNameQuery(operation.TeamName));

            if (request.IsFailure)
                continue;

            teamOperation.TeamId = request.Value.Id.ToString();

            returnData.Add(teamOperation);
        }

        foreach (StudentOfferingMSTeamOperation operation in operations.StudentOfferingOperations)
        {
            Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(operation.StudentId));

            if (student.IsFailure)
                continue;

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = student.Value.EmailAddress
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

            Result<TeamResource> request = await _mediator.Send(new GetTeamByNameQuery(operation.TeamName));

            if (request.IsFailure)
                continue;

            teamOperation.TeamId = request.Value.Id.ToString();

            returnData.Add(teamOperation);
        }

        foreach (StudentMSTeamOperation operation in operations.StudentOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = operation.Student.EmailAddress
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

            teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

            returnData.Add(teamOperation);
        }

        foreach (TeacherMSTeamOperation operation in operations.TeacherOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = operation.Staff.EmailAddress
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

            teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

            returnData.Add(teamOperation);
        }

        foreach (CasualMSTeamOperation operation in operations.CasualOperations)
        {
            Casual casual = await _casualRepository.GetById(CasualId.FromValue(operation.CasualId));

            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                UserEmail = casual.EmailAddress
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

            teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

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

            teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

            returnData.Add(teamOperation);
        }

        foreach (GroupTutorialCreatedMSTeamOperation operation in operations.TutorialOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = $"AC - {operation.GroupTutorial.EndDate:yyyy} - {operation.GroupTutorial.Name}",
                Action = "Group"
            };

            returnData.Add(teamOperation);
        }

        foreach (StudentEnrolledMSTeamOperation operation in operations.EnrolmentOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = operation.Student.EmailAddress,
                AdditionalInformation = operation.Student.CurrentGrade.ToString()
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

            teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.TeamName, Year = operation.TeamName });

            returnData.Add(teamOperation);
        }

        foreach (TeacherEmployedMSTeamOperation operation in operations.EmploymentOperations)
        {
            TeamsOperation teamOperation = new()
            {
                Id = operation.Id,
                TeamName = operation.TeamName,
                UserEmail = operation.Staff.EmailAddress,
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

            Result<TeamResource> team = await _mediator.Send(new GetTeamByNameQuery(operation.TeamName));

            if (team.IsFailure)
                continue;

            teamOperation.TeamId = team.Value.Id.ToString();

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

            Result<TeamResource> team = await _mediator.Send(new GetTeamByNameQuery(operation.TeamName));

            if (team.IsFailure)
                continue;

            teamOperation.TeamId = team.Value.Id.ToString();

            returnData.Add(teamOperation);
        }

        return returnData;
    }

    // POST api/Operations/Complete
    [Route("Complete/{id}")]
    [HttpPost]
    public async Task Complete(int id)
    {
        MSTeamOperation operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

        if (operation != null)
        {
            operation.Complete();
            await _unitOfWork.CompleteAsync();
        }
    }
}