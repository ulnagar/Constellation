namespace Constellation.Application.Interfaces.Gateways.TeamsGateway;

using Models;
using System.Collections.Generic;

public interface ITeamsGateway
{
    void Connect();

    /// <summary>
    /// Gets all Teams that are not marked Archived for the specific user.
    /// </summary>
    /// <param name="userEmail">Email address of the user who logged into the Teams system.</param>
    List<Team> GetTeams(string userEmail);
    List<TeamMember> GetTeamMembers(string groupId);
    List<TeamChannel> GetChannels(string groupId);
    List<TeamMember> GetChannelMembers(string groupId, string channelName);
    void AddTeam(string teamName, string teamDescription, bool isClassTeam);
    void ArchiveTeam(string groupId);
    void RemoveTeam(string groupId);
    void AddTeamMember(string groupId, string userEmail);
    void AddTeamOwner(string groupId, string userEmail);
    void RemoveTeamMember(string groupId, string userEmail);
    void DemoteTeamOwner(string groupId, string userEmail);
    void AddChannel(string groupId, string channelName, bool isPrivate);
    void RemoveChannel(string groupId, string channelName);
    void AddChannelMember(string groupId, string channelName, string userEmail);
    void AddChannelOwner(string groupId, string channelName, string userEmail);
    void RemoveChannelMember(string groupId, string channelName, string userEmail);
    void DemoteChannelOwner(string groupId, string channelName, string userEmail);
}