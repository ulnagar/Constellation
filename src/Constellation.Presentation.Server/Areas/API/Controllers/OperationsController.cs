namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Domains.LinkedSystems.Teams.Models;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.LinkedSystems;
using Core.Abstractions.Repositories;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading;

[ApiController]
public sealed class OperationsController : ControllerBase
{
    private readonly ITeamOperationRepository _teamOperationRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OperationsController(
        ITeamOperationRepository teamOperationRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork)
    {
        _teamOperationRepository = teamOperationRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
    }

    #region v1 Operations

    // GET api/Operations/Due
    [HttpGet("api/v1/operations/due")]
    public async Task<IEnumerable<TeamsOperationDto>> GetDue()
    {
        List<TeamOperation> newOperations = await _teamOperationRepository.GetDue();

        return await BuildOperations(newOperations);
    }


    // GET api/Operations/Overdue
    [HttpGet("api/v1/Operations/Overdue")]
    public async Task<IEnumerable<TeamsOperationDto>> GetOverdue()
    {
        List<TeamOperation> newOperations = await _teamOperationRepository.GetOverdue();

        return await BuildOperations(newOperations);
    }

    private async Task<ICollection<TeamsOperationDto>> BuildOperations(
        List<TeamOperation> newOperations)
    {
        List<TeamsOperationDto> returnData = new();

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
                ModifyTeamMembershipTeamOperation modifyMembership => new TeamsOperationDto()
                {
                    Id = modifyMembership.Id,
                    TeamId = modifyMembership.TeamId.ToString(),
                    UserEmail = modifyMembership.UserId.Email,
                    Action = modifyMembership.Action == TeamAction.Remove ? "Remove" : "Add",
                    Role = modifyMembership.Action == TeamAction.AddOwner ? "Owner" : "Member"
                },
                ModifyTeamChannelMembershipTeamOperation channelMembership => new TeamsOperationDto()
                {
                    Id = channelMembership.Id,
                    TeamId = channelMembership.TeamId.ToString(),
                    UserEmail = channelMembership.UserId.Email,
                    AdditionalInformation = channelMembership.ChannelName,
                    Action = channelMembership.Action == TeamAction.Remove ? "RemoveChannel" : "AddChannel",
                    Role = channelMembership.Action == TeamAction.AddOwner ? "Owner" : "Member"
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

        Team? exactMatch = teams.FirstOrDefault(team => team.Name == name);

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