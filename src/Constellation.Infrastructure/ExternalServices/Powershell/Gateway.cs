#pragma warning disable CA1002
namespace Constellation.Infrastructure.ExternalServices.Powershell;

using Application.Exceptions;
using Application.Interfaces.Gateways.PowershellGateway.Models;
using Constellation.Application.Interfaces.Gateways.PowershellGateway;
using Microsoft.PowerShell;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security;

public class Gateway : IPowershellGateway
{
    private readonly Runspace _runspace;

    private string Username { get; init; }
    private SecureString Password { get; init; }

    public Gateway()
    {
        InitialSessionState initial = InitialSessionState.CreateDefault();
        initial.ExecutionPolicy = ExecutionPolicy.Unrestricted;

        //if (OperatingSystem.IsWindows())
        //{
        //    bool imported = false;
        //    string? paths = Environment.GetEnvironmentVariable("PSModulePath");
        //    if (paths is not null)
        //    {
        //        List<string> locations = paths.Split(';').ToList();
        //        foreach (string location in locations)
        //        {
        //            string path = $@"{location}\MicrosoftTeams\6.9.0";

        //            if (Directory.Exists(Environment.ExpandEnvironmentVariables(path)))
        //            {
        //                initial.ImportPSModulesFromPath(Environment.ExpandEnvironmentVariables(path));
        //                imported = true;
        //            }
        //        }
        //    }

        //    if (imported is false)
        //    {
        //        throw new ModuleNotFoundException("Could not locate the MicrosoftTeams powershell module in any of the PSModulePath environment paths.");
        //    }
        //}

        _runspace = RunspaceFactory.CreateRunspace(initial);
        _runspace.Open();

        PowerShell ps = PowerShell.Create(_runspace);
        ps.AddCommand("Import-Module")
            .AddParameter("Name", "MicrosoftTeams")
            .Invoke();

        ps.Dispose();

        Username = "";
        Password = new NetworkCredential("", "").SecurePassword;
    }

    public void Connect(string username, SecureString password)
    {
        PowerShell ps = PowerShell.Create(_runspace);

        IEnumerable<PSModuleInfo> modules = ps
            .AddCommand("Get-Module")
            .Invoke<PSModuleInfo>();

        PSCredential credential = new(Username, Password);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        //ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Connect-MicrosoftTeams")
            .AddParameter("Credential", credential);

        ps.Invoke();

        ps.Dispose();
    }

    public List<Team> GetTeams(string userEmail)
    {
        PowerShell ps = PowerShell.Create(_runspace);

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.AddStatement()
            .AddCommand("Get-Team")
            .AddParameter("User", userEmail)
            .AddParameter("Archived", false);

        Collection<PSObject> results = ps.Invoke();

        ps.Dispose();

        return ConvertObjects<Team>(results);
    }

    public List<TeamMember> GetTeamMembers(string groupId)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Get-TeamUser")
            .AddParameter("GroupId", groupId);

        Collection<PSObject> results = ps.Invoke();
        
        ps.Dispose();
        
        return ConvertObjects<TeamMember>(results);
    }

    public List<TeamChannel> GetChannels(string groupId)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Get-TeamAllChannel")
            .AddParameter("GroupId", groupId);

        Collection<PSObject> results = ps.Invoke();

        ps.Dispose();

        return ConvertObjects<TeamChannel>(results);
    }

    public List<TeamMember> GetChannelMembers(string groupId, string channelName)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Get-TeamChannelUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName);

        Collection<PSObject> results = ps.Invoke();

        ps.Dispose();

        return ConvertObjects<TeamMember>(results);
    }

    public void AddTeam(string teamName, string teamDescription, bool isClassTeam) { }

    public void ArchiveTeam(string groupId) { }

    public void RemoveTeam(string groupId) { }

    public void AddTeamMember(string groupId, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Add-TeamUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("User", userEmail)
            .AddParameter("Role", "Member");

        ps.Invoke();
        
        ps.Dispose();
    }

    public void AddTeamOwner(string groupId, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Add-TeamUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("User", userEmail)
            .AddParameter("Role", "Owner");

        ps.Invoke();

        ps.Dispose();
    }

    public void RemoveTeamMember(string groupId, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Remove-TeamUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("User", userEmail);

        ps.Invoke();

        ps.Dispose();
    }

    public void DemoteTeamOwner(string groupId, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Remove-TeamUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("User", userEmail)
            .AddParameter("Role", "Owner");

        ps.Invoke();

        ps.Dispose();
    }

    public void AddChannel(string groupId, string channelName, bool isPrivate)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("New-TeamChannel")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName);

        if (isPrivate)
        {
            ps.AddParameter("MembershipType", "Private");
        }

        ps.Invoke();

        ps.Dispose();
    }

    public void RemoveChannel(string groupId, string channelName)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Remove-TeamChannel")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName);

        ps.Invoke();

        ps.Dispose();
    }

    public void AddChannelMember(string groupId, string channelName, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Add-TeamChannelUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName)
            .AddParameter("User", userEmail);

        ps.Invoke();

        ps.Dispose();
    }

    public void AddChannelOwner(string groupId, string channelName, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Add-TeamChannelUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName)
            .AddParameter("User", userEmail)
            .AddParameter("Role", "Owner");

        ps.Invoke();

        ps.Dispose();
    }

    public void RemoveChannelMember(string groupId, string channelName, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Remove-TeamChannelUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName)
            .AddParameter("User", userEmail);

        ps.Invoke();

        ps.Dispose();
    }

    public void DemoteChannelOwner(string groupId, string channelName, string userEmail)
    {
        PowerShell ps = PowerShell.Create();

        ps.Streams.Error.DataAdded += ErrorEventHandler;

        ps.Runspace = _runspace;
        ps.AddStatement()
            .AddCommand("Remove-TeamChannelUser")
            .AddParameter("GroupId", groupId)
            .AddParameter("DisplayName", channelName)
            .AddParameter("User", userEmail);

        ps.Invoke();

        ps.Dispose();
    }

    public void Dispose()
    {
        _runspace.Close();
        _runspace.Dispose();
    }

    private List<T> ConvertObjects<T>(ICollection<PSObject> results)
    {
        List<T> returnData = [];

        foreach (PSObject obj in results)
        {
            string objectString = JsonConvert.SerializeObject(obj.Properties.ToDictionary(k => k.Name, k => k.Value));

            if (!string.IsNullOrWhiteSpace(objectString))
            {
                T convertedObject = JsonConvert.DeserializeObject<T>(objectString);
                returnData.Add(convertedObject);
            }
        }

        return returnData;
    }

    private static void ErrorEventHandler(object? sender, DataAddedEventArgs? e)
    {
        if (sender is not null && e is not null)
            Console.WriteLine(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString());
    }

}
