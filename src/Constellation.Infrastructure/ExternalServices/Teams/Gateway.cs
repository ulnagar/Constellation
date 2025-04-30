#pragma warning disable CA1002
#nullable enable
namespace Constellation.Infrastructure.ExternalServices.Teams;

using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways.TeamsGateway;
using Application.Interfaces.Gateways.TeamsGateway.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.PowerShell;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

public class Gateway : ITeamsGateway
{
    private readonly ILogger _logger;
    private readonly TeamsGatewayConfiguration _settings;
    private readonly Runspace _runspace;
    
    public Gateway(
        IOptions<TeamsGatewayConfiguration> settings,
        ILogger logger)
    {
        _logger = logger.ForContext<ITeamsGateway>();
        _settings = settings.Value;

        if (!_settings.IsConfigured())
        {
            _logger
                .ForContext(nameof(TeamsGatewayConfiguration), _settings, true)
                .Error("Failed to retrieve Teams Gateway configuration!");

            throw new InvalidConfigurationException();
        }
        
        InitialSessionState initial = InitialSessionState.CreateDefault();
        initial.ExecutionPolicy = ExecutionPolicy.Unrestricted;

        _runspace = RunspaceFactory.CreateRunspace(initial);
        _runspace.Open();

        using PowerShell ps = PowerShell.Create(_runspace);
        ps.AddCommand("Import-Module")
            .AddParameter("Name", "MicrosoftTeams")
            .Invoke();
    }

    private PSCredential? GetCredentials()
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            string script = $"New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList {_settings.Username}, (Get-Content {_settings.PasswordFile} | ConvertTo-SecureString -Key (Get-Content {_settings.KeyFile}))";

            List<PSCredential> credentials = ps.AddStatement()
                .AddScript(script)
                .Invoke<PSCredential>()
                .ToList();

            return credentials.FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: GetCredentials");

            return null;
        }
    }

    public void Connect()
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        PSCredential? credential = GetCredentials();

        if (credential is null)
            throw new InvalidConfigurationException();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.AddStatement()
            .AddCommand("Connect-MicrosoftTeams")
            .AddParameter("Credential", credential)
            .Invoke();
    }

    public List<Team> GetTeams()
    {
        using PowerShell ps = PowerShell.Create(_runspace);
        
        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            Collection<PSObject> results = ps
                .AddCommand("Get-Team")
                .AddParameter("User", _settings.Username)
                .AddParameter("Archived", false)
                .Invoke<PSObject>();

            return ConvertObjects<Team>(results);
        }
        catch (Exception e)
        {
            _logger
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: GetTeams");

            return [];
        }
    }

    public List<TeamMember> GetTeamMembers(string groupId)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            Collection<PSObject> results = ps.AddCommand("Get-TeamUser")
                .AddParameter("GroupId", groupId)
                .Invoke<PSObject>();

            return ConvertObjects<TeamMember>(results);
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: GetTeamMembers");

            return [];
        }
    }

    public List<TeamChannel> GetChannels(string groupId)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            Collection<PSObject> results = ps.AddCommand("Get-TeamAllChannel")
                .AddParameter("GroupId", groupId)
                .Invoke();

            return ConvertObjects<TeamChannel>(results);
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: GetChannels");

            return [];
        }
    }

    public List<TeamMember> GetChannelMembers(string groupId, string channelName)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            Collection<PSObject> results = ps.AddCommand("Get-TeamChannelUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .Invoke<PSObject>();

            return ConvertObjects<TeamMember>(results);
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: GetChannelMembers");

            return [];
        }
    }

    public void AddTeam(string teamName, string teamDescription, bool isClassTeam) { }

    public void ArchiveTeam(string groupId) { }

    public void RemoveTeam(string groupId) { }

    public void AddTeamMember(string groupId, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Add-TeamUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("User", userEmail)
                .AddParameter("Role", "Member")
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: AddTeamMember");
        }
    }

    public void AddTeamOwner(string groupId, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Add-TeamUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("User", userEmail)
                .AddParameter("Role", "Owner")
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: AddTeamOwner");
        }
    }

    public void RemoveTeamMember(string groupId, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Remove-TeamUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("User", userEmail)
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: RemoveTeamMember");
        }
    }

    public void DemoteTeamOwner(string groupId, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Remove-TeamUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("User", userEmail)
                .AddParameter("Role", "Owner")
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: DemoteTeamOwner");
        }
    }

    public void AddChannel(string groupId, string channelName, bool isPrivate)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("New-TeamChannel")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName);

            if (isPrivate)
            {
                ps.AddParameter("MembershipType", "Private");
            }

            ps.Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext("IsPrivate", isPrivate)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: AddChannel");
        }
    }

    public void RemoveChannel(string groupId, string channelName)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Remove-TeamChannel")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: RemoveChannel");
        }
    }

    public void AddChannelMember(string groupId, string channelName, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Add-TeamChannelUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .AddParameter("User", userEmail)
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: AddChannelMember");
        }
    }

    public void AddChannelOwner(string groupId, string channelName, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Add-TeamChannelUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .AddParameter("User", userEmail)
                .AddParameter("Role", "Owner")
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: AddChannelOwner");
        }
    }

    public void RemoveChannelMember(string groupId, string channelName, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Remove-TeamChannelUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .AddParameter("User", userEmail)
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: RemoveChannelMember");
        }
    }

    public void DemoteChannelOwner(string groupId, string channelName, string userEmail)
    {
        using PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        try
        {
            ps.AddStatement()
                .AddCommand("Remove-TeamChannelUser")
                .AddParameter("GroupId", groupId)
                .AddParameter("DisplayName", channelName)
                .AddParameter("User", userEmail)
                .Invoke();
        }
        catch (Exception e)
        {
            _logger
                .ForContext("GroupId", groupId)
                .ForContext("ChannelName", channelName)
                .ForContext("UserEmail", userEmail)
                .ForContext(nameof(Exception), e, true)
                .Error("Teams Gateway Error: DemoteChannelOwner");
        }
    }

    public void Dispose()
    {
        _runspace.Close();
        _runspace.Dispose();
    }

    private static List<T> ConvertObjects<T>(ICollection<PSObject> results)
    {
        List<T> returnData = [];

        foreach (PSObject obj in results)
        {
            string objectString = JsonConvert.SerializeObject(obj.Properties.ToDictionary(k => k.Name, k => k.Value));

            if (!string.IsNullOrWhiteSpace(objectString))
            {
                T? convertedObject = JsonConvert.DeserializeObject<T>(objectString);

                if (convertedObject is not null)
                    returnData.Add(convertedObject);
            }
        }

        return returnData;
    }

    private void ErrorEventHandler(object? sender, DataAddedEventArgs? e)
    {
        if (sender is not null && e is not null)
            _logger
                .Warning(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString());
    }

}
