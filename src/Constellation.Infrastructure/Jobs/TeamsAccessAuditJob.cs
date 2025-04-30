namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Application.Teams.GetCurrentTeamsWithMembership;
using Constellation.Application.Interfaces.Gateways.TeamsGateway;
using Constellation.Application.Interfaces.Gateways.TeamsGateway.Models;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal sealed class TeamsAccessAuditJob : ITeamsAccessAuditJob
{
    private readonly IMediator _mediator;
    private readonly ITeamsGateway _gateway;
    private readonly ILogger _logger;

    private bool _processChannels;

    public TeamsAccessAuditJob(
        IMediator mediator,
        ITeamsGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _gateway = gateway;
        _logger = logger
            .ForContext<ITeamsAccessAuditJob>();

        _processChannels = false;
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger
            .Information("Start Teams Access Audit Job");

        _gateway.Connect();

        Result<List<TeamWithMembership>> serverTeamsRequest = await _mediator.Send(new GetCurrentTeamsWithMembershipQuery(), cancellationToken);

        if (serverTeamsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), serverTeamsRequest.Error, true)
                .Warning("Failed to retrieve list of expected Teams");
        }
        
        foreach (var team in serverTeamsRequest.Value)
        {
            _logger
                .Information("Processing Team {TeamName}", team.Name);
            
            List<TeamMember> teamMembers = _gateway.GetTeamMembers(team.Id.ToString());

            foreach (var expectedUser in team.Members)
            {
                await CheckExpectedUserTeamAccess(team.Id, expectedUser, teamMembers);
            }

            foreach (var foundUser in teamMembers)
            {
                await CheckFoundUserTeamAccess(team.Id, foundUser, team.Members);
            }

            if (!_processChannels)
                continue;

            List<TeamChannel> channels = _gateway.GetChannels(team.Id.ToString());

            foreach (TeamChannel channel in channels.Where(channel => channel.MembershipType == TeamChannelMembershipType.Private))
            {
                _logger
                    .Information("Processing Private Channel: {ChannelName}", channel.DisplayName);

                List<TeamMember> channelMembers = _gateway.GetChannelMembers(team.Id.ToString(), channel.DisplayName);

                if (team.Description.Split(';').Contains("CLASS"))
                {
                    List<TeamWithMembership.Member> expectedOwners = team.Members
                        .Where(user => user.PermissionLevel == "Owner")
                        .ToList();

                    foreach (var expectedOwner in expectedOwners)
                    {
                        await CheckExpectedUserChannelAccess(team.Id, channel.DisplayName, expectedOwner, channelMembers);
                    }

                    foreach (var foundUser in channelMembers.Where(member => member.Role == TeamMemberRole.Owner))
                    {
                        await CheckFoundUserChannelAccess(team.Id, channel.DisplayName, foundUser, expectedOwners);
                    }
                }
                else if (team.Description.Split(';').Contains("STUDENTS"))
                {
                    List<TeamWithMembership.Member> expectedOwners = team.Members
                        .Where(user => user.Channels
                            .Any(channelPermission =>
                                channelPermission.Key == channel.DisplayName &&
                                channelPermission.Value == "Owner"))
                        .ToList();

                    foreach (var expectedOwner in expectedOwners)
                    {
                        await CheckExpectedUserChannelAccess(team.Id, channel.DisplayName, expectedOwner, channelMembers);
                    }

                    foreach (var foundUser in channelMembers.Where(member => member.Role == TeamMemberRole.Owner))
                    {
                        await CheckFoundUserChannelAccess(team.Id, channel.DisplayName, foundUser, expectedOwners);
                    }
                }
            }
        }
    }

    private async Task CheckExpectedUserTeamAccess(Guid teamId, TeamWithMembership.Member expectedUser, List<TeamMember> foundUsers)
    {
        TeamMember matchingFoundUser = foundUsers
            .FirstOrDefault(user =>
                string.Equals(user.User, expectedUser.EmailAddress, StringComparison.InvariantCultureIgnoreCase));

        if (matchingFoundUser is null)
        {
            _logger
                .Information("Found user that needs to be added: {EmailAddress}", expectedUser.EmailAddress);

            //_gateway.AddTeamMember(teamId.ToString(), expectedUser.EmailAddress);

            if (expectedUser.PermissionLevel == "Owner")
            {
                _logger
                    .Information("User must be Owner: {EmailAddress}", expectedUser.EmailAddress);

                //_gateway.AddTeamOwner(teamId.ToString(), expectedUser.EmailAddress);
            }
            
            return;
        }

        switch (matchingFoundUser.Role)
        {
            case TeamMemberRole.Member when expectedUser.PermissionLevel == "Owner":
                _logger
                    .Information("User must be Owner: {EmailAddress}", expectedUser.EmailAddress);

                //_gateway.AddTeamOwner(teamId.ToString(), expectedUser.EmailAddress);
                break;
            case TeamMemberRole.Owner when expectedUser.PermissionLevel == "Member":
                _logger
                    .Information("User must NOT be an Owner: {EmailAddress}", expectedUser.EmailAddress);

                //_gateway.DemoteTeamOwner(teamId.ToString(), expectedUser.EmailAddress);
                break;
        }
    }

    private async Task CheckFoundUserTeamAccess(Guid teamId, TeamMember foundUser, List<TeamWithMembership.Member> expectedUsers)
    {
        TeamWithMembership.Member expectedUser = expectedUsers
            .FirstOrDefault(user =>
                string.Equals(user.EmailAddress, foundUser.User, StringComparison.InvariantCultureIgnoreCase));

        if (expectedUser is null)
        {
            _logger
                .Information("Found user that needs to be removed: {EmailAddress}", foundUser.User);

            if (foundUser.Role == TeamMemberRole.Owner)
            {
                //_gateway.DemoteTeamOwner(teamId.ToString(), foundUser.User);
            }

            //_gateway.RemoveTeamMember(teamId.ToString(), foundUser.User);

            return;
        }

        switch (expectedUser.PermissionLevel)
        {
            case "Member" when foundUser.Role == TeamMemberRole.Owner:
                _logger
                    .Information("User must be an Owner: {EmailAddress}", foundUser.User);

                //_gateway.AddTeamOwner(teamId.ToString(), expectedUser.EmailAddress);
                break;
            case "Owner" when foundUser.Role == TeamMemberRole.Member:
                _logger
                    .Information("User must NOT be an Owner: {EmailAddress}", foundUser.User);

                //_gateway.DemoteTeamOwner(teamId.ToString(), expectedUser.EmailAddress);
                break;
        }
    }

    private async Task CheckExpectedUserChannelAccess(Guid teamId, string channelName, TeamWithMembership.Member expectedOwner, List<TeamMember> foundUsers)
    {
        TeamMember matchingFoundUser = foundUsers
            .FirstOrDefault(user =>
                string.Equals(user.User, expectedOwner.EmailAddress, StringComparison.InvariantCultureIgnoreCase));

        if (matchingFoundUser is null)
        {
            _logger
                .Information("Found user that needs to be added to channel as Owner: {ChannelName} - {EmailAddress}", channelName, expectedOwner.EmailAddress);

            //_gateway.AddChannelMember(teamId.ToString(), channelName, expectedOwner.EmailAddress);
            //_gateway.AddChannelOwner(teamId.ToString(), channelName, expectedOwner.EmailAddress);

            return;
        }

        if (matchingFoundUser.Role == TeamMemberRole.Member)
        {
            _logger
                .Information("User must be an Owner of Channel: {ChannelName} - {EmailAddress}", channelName, expectedOwner.EmailAddress);

            //_gateway.AddChannelOwner(teamId.ToString(), channelName, expectedOwner.EmailAddress);
        }
    }

    private async Task CheckFoundUserChannelAccess(Guid teamId, string channelName, TeamMember foundUser, List<TeamWithMembership.Member> expectedOwners)
    {
        TeamWithMembership.Member matchingExpectedOwner = expectedOwners
            .FirstOrDefault(user =>
                string.Equals(user.EmailAddress, foundUser.User, StringComparison.InvariantCultureIgnoreCase));

        if (matchingExpectedOwner is null)
        {
            _logger
                .Information("Found user that needs to be removed from channel: {ChannelName} - {EmailAddress}", channelName, foundUser.User);

            //_gateway.DemoteChannelOwner(teamId.ToString(), channelName, foundUser.User);
            //_gateway.RemoveChannelMember(teamId.ToString(), channelName, foundUser.User);

            return;
        }

        if (foundUser.Role == TeamMemberRole.Member)
        {
            _logger
                .Information("User must be an Owner of Channel: {ChannelName} - {EmailAddress}", channelName, foundUser.User);

            //_gateway.AddChannelOwner(teamId.ToString(), channelName, foundUser.User);
        }
    }

}
