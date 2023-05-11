namespace Constellation.Infrastructure.ExternalServices.AdobeConnect;

using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Models;
using Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

internal class Gateway : IAdobeConnectGateway
{
    private readonly HttpClient _client;
    private readonly AdobeConnectGatewayConfiguration _settings;
    private DateTime? _loginTime;
    private bool _loginInProgress;

    private readonly bool _logOnly = true;
    private readonly ILogger _logger;

    public Gateway(
        IOptions<AdobeConnectGatewayConfiguration> settings,
        Serilog.ILogger logger)
    {
        _logger = logger.ForContext<IAdobeConnectGateway>();

        _settings = settings.Value;

        _logOnly = !_settings.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        // Only configure the client if the _logOnly setting is false
        _client = new HttpClient
        {
            BaseAddress = new Uri(_settings.ServerUrl)
        };
    }

    private async Task CheckLogin()
    {
        while (_loginInProgress)
            await Task.Delay(1000);

        if (!_loginTime.HasValue || _loginTime.Value.AddMinutes(25) <= DateTime.Now)
        {
            _loginInProgress = true;
            await Login();
            _loginInProgress = false;
        }
    }

    private async Task Login()
    {
        var loginParams = new NameValueCollection
        {
            { "login", _settings.Username },
            { "password", _settings.Password }
        };

        var loginQuery = BuildQuery("login", loginParams);

        if (_logOnly)
        {
            _logger.Information("Login: {@query}", loginQuery);

            _loginTime = DateTime.Now;

            return;
        }

        var loginResponse = await _client.GetAsync(loginQuery);

        var cookie = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault() ?? throw new Exception("Could not log in to Adobe Connect");
        var tokens = cookie.Split(';');
        _client.DefaultRequestHeaders.Add("Cookie", tokens[0]);

        _loginTime = DateTime.Now;
    }

    private static string BuildQuery(string action, NameValueCollection @params)
    {
        var paramString = "";

        foreach (var key in @params.AllKeys)
        {
            var values = @params.GetValues(key);

            foreach (var value in values)
            {
                if (key == "name")
                {
                    paramString += $"&{key}={WebUtility.UrlEncode(value)}";
                }
                else
                {
                    paramString += $"&{key}={value}";
                }
            }
        }

        return $"/api/xml?action={action}{(paramString != "" ? paramString : "")}";
    }

    private async Task<T> Request<T>(string action, NameValueCollection queryParams) where T : ResponseDto
    {
        await CheckLogin();

        var queryUri = BuildQuery(action, queryParams);

        if (_logOnly)
        {
            _logger.Information("Request<T>: {@query}", queryUri);

            return new ResponseDto() as T;
        }

        var response = await _client.GetAsync(queryUri);
        var resultStream = await response.Content.ReadAsStreamAsync();
        var sr = new StreamReader(resultStream);

        try
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute("results"));
            return (T)serializer.Deserialize(sr);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private bool IsResponseValid(ResponseDto response)
    {
        if (_logOnly)
            return true;

        if (response == null || response.Status.Code != "ok")
            return false;

        return true;
    }

    private async Task<string> FindFolder(string year, string grade)
    {
        if (_logOnly)
        {
            _logger.Information("FindFolder: year={year}, grade={grade}", year, grade);

            return "1";
        }

        var yearFolderParams = new NameValueCollection
        {
            {"sco-id", _settings.BaseFolder},
            {"filter-type", "folder"},
            {"filter-name", year}
        };

        var yearResponse = await Request<FolderLookup>("sco-contents", yearFolderParams);

        if (yearResponse.Scos.Count == 1)
        {
            var gradeFolderParams = new NameValueCollection
            {
                {"sco-id", yearResponse.Scos.First().ScoId},
                {"filter-type", "folder"},
                {"filter-name", grade}
            };

            var gradeResponse = await Request<FolderLookup>("sco-contents", gradeFolderParams);

            if (gradeResponse.Scos.Count == 1)
            {
                return gradeResponse.Scos.First().ScoId;
            }

            return await CreateFolder(yearResponse.Scos.First().ScoId, grade);
        }

        var folderId = await CreateFolder(_settings.BaseFolder, year);
        return await CreateFolder(folderId, grade);
    }

    private async Task<string> CreateFolder(string sco, string name)
    {
        if (_logOnly)
        {
            _logger.Information("CreateFolder: sco={sco}, name={name}", sco, name);

            return "1";
        }

        var actionParams = new NameValueCollection
        {
            {"type", "folder"},
            {"name", name},
            {"folder-id", sco}
        };

        var response = await Request<RoomCreation>("sco-update", actionParams);

        return response.Sco.ScoId;
    }

    private async Task AddDefaultRoomPermissions(string scoId, string faculty)
    {
        if (_logOnly)
        {
            _logger.Information("AddDefaultRoomPermissions: scoId={scoId}, faculty={faculty}", scoId, faculty);

            return;
        }

        var groupList = new List<string>();

        _settings.Groups.TryGetValue("Administration", out string adminGroup);
        if (!string.IsNullOrWhiteSpace(adminGroup))
            groupList.Add(adminGroup);

        _settings.Groups.TryGetValue("Executive", out string execGroup);
        if (!string.IsNullOrWhiteSpace(execGroup))
            groupList.Add(execGroup);

        _settings.Groups.TryGetValue(faculty, out string facultyGroup);
        if (!string.IsNullOrWhiteSpace(facultyGroup))
            groupList.Add(facultyGroup);

        foreach (var group in groupList)
        {
            var permissionParams = new NameValueCollection
            {
                {"acl-id", scoId},
                {"principal-id", group},
                {"permission-id", "host"}
            };

            await Request<ResponseDto>("permissions-update", permissionParams);
        }
    }

    public async Task<string> GetUserPrincipalId(string username)
    {
        var userParams = new NameValueCollection
        {
            { "filter-login", username + "@DETNSW" },
            { "filter-login", username + "@det.nsw.edu.au" }
        };

        if (_logOnly)
        {
            _logger.Information("GetUserPrincipalId: username={username}, userParams={@userParams}", username, userParams.ToDictionary());

            return "1";
        }

        var response = await Request<UserLookup>("principal-list", userParams);

        var principal = response.PrincipalList.FirstOrDefault();

        return principal?.PrincipalId;
    }

    public async Task<bool> UserPermissionUpdate(string principalId, string scoId, string accessLevel)
    {
        var actionParams = new NameValueCollection
        {
            { "acl-id", scoId },
            { "principal-id", principalId },
            { "permission-id", accessLevel }
        };

        if (_logOnly)
        {
            _logger.Information("UserPermissionUpdate: principalId={principalId}, scoId={scoId}, accessLevel={accessLevel}, actionParams={@actionParams}", principalId, scoId, accessLevel, actionParams.ToDictionary());

            return true;
        }

        var response = await Request<ResponseDto>("permissions-update", actionParams);
        var code = response.Status.Code;

        return code == "ok";
    }

    public async Task<bool> GroupMembershipUpdate(string principalId, string groupId, string accessLevel)
    {
        var isMember = accessLevel == AdobeConnectAccessLevel.Remove ? "false" : "true";

        var actionParams = new NameValueCollection
        {
            {"group-id", groupId},
            {"principal-id", principalId},
            {"is-member", isMember}
        };

        if (_logOnly)
        {
            _logger.Information("GroupMembershipUpdate: principalId={principalId}, groupId={groupId}, accessLevel={accessLevel}, actionParams={@actionParams}", principalId, groupId, accessLevel, actionParams.ToDictionary());

            return true;
        }

        var response = await Request<ResponseDto>("group-membership-update", actionParams);

        var code = response.Status.Code;

        return code == "ok";
    }

    public async Task<ICollection<string>> GetSessionsForDate(string scoId, DateTime sessionDate)
    {
        var dateOfSession = sessionDate.ToString("yyyy-MM-dd");
        var dateAfterSession = sessionDate.AddDays(1).ToString("yyyy-MM-dd");

        var sessionParams = new NameValueCollection
        {
            {"sco-id", scoId},
            {"filter-gte-date-created", dateOfSession },
            {"filter-lt-date-created", dateAfterSession }
        };

        if (_logOnly)
        {
            _logger.Information("GetSessionsForDate: scoId={scoId}, sessionDate={sessionDate}, sessionParams={@sessionParams}", scoId, sessionDate, sessionParams.ToDictionary());

            return new List<string>() { "1", "2", "3" };
        }

        var response = await Request<SessionLookup>("report-meeting-sessions", sessionParams);

        return response?.Sessions.Select(s => s.AssetId).ToList();
    }

    public async Task<ICollection<string>> GetSessionUsers(string scoId, string assetId)
    {
        var userParams = new NameValueCollection
        {
            {"sco-id", scoId},
            {"filter-asset-id", assetId}
        };

        if (_logOnly)
        {
            _logger.Information("GetSessionUsers: scoId={scoId}, assetId={assetId}, userParams={@userParams}", scoId, assetId, userParams.ToDictionary());

            return new List<string>() { "1", "2", "3" };
        }

        var response = await Request<AttendanceLookup>("report-meeting-attendance", userParams);

        return response.Users.Where(u => u.DateLeft == null).Select(u => u.PrincipalId).ToList();
    }

    public async Task<ICollection<AdobeConnectSessionUserDetailDto>> GetSessionUserDetails(string scoId, string assetId)
    {
        var userParams = new NameValueCollection
        {
            {"sco-id", scoId},
            {"filter-asset-id", assetId}
        };

        if (_logOnly)
        {
            _logger.Information("GetSessionUserDetails: scoId={scoId}, assetId={assetId}, userParams={@userParams}", scoId, assetId, userParams.ToDictionary());

            return new List<AdobeConnectSessionUserDetailDto>() { new() { Name = "Sample User", Login = "sample.user@detnsw", LoginTime = DateTime.Now, LogoutTime = null } };
        }

        var response = await Request<AttendanceLookup>("report-meeting-attendance", userParams);

        var returnData = new List<AdobeConnectSessionUserDetailDto>();

        foreach (var user in response.Users)
        {
            var userDetails = new AdobeConnectSessionUserDetailDto
            {
                Name = user.Name,
                Login = user.Login,
                LoginTime = user.DateEntered,
                LogoutTime = user.DateLeft
            };

            returnData.Add(userDetails);
        }

        return returnData;
    }

    public async Task<AdobeConnectRoomDto> CreateRoom(string name, string dateStart, string dateEnd, string urlPath, bool detectParentFolder, string parentFolder, string year, string grade, bool useTemplate, string faculty)
    {
        var actionParams = new NameValueCollection
        {
            {"type", "meeting"},
            {"name", name},
            {"date-begin", dateStart},
            {"date-end", dateEnd},
            {"url-path", urlPath}
        };

        if (useTemplate)
        {
            actionParams.Add("source-sco-id", _settings.TemplateSco);
        }

        if (detectParentFolder)
        {
            var folderId = await FindFolder(year, grade);

            actionParams.Add("folder-id", folderId);
        }
        else
        {
            actionParams.Add("folder-id", parentFolder);
        }

        RoomCreation response;

        if (_logOnly)
        {
            _logger.Information("CreateRoom: args={@args}, actionParams={@actionParams}", new { name, dateStart, dateEnd, urlPath, detectParentFolder, parentFolder, year, grade, useTemplate, faculty}, actionParams.ToDictionary());

            response = new() { Sco = new() { Name = name, ScoId = "1", UrlPath = urlPath }, Status = new() { Code = "ok" } };
        }
        else
        {
            response = await Request<RoomCreation>("sco-update", actionParams);

            if (response.Status.Code.ToLower() != "ok")
            {
                return null;
            }
        }

        await AddDefaultRoomPermissions(response.Sco.ScoId, faculty);

        var newRoom = new AdobeConnectRoomDto
        {
            ScoId = response.Sco.ScoId,
            Name = response.Sco.Name,
            UrlPath = _settings.ServerUrl + response.Sco.UrlPath
        };

        return newRoom;
    }

    public async Task<ICollection<AdobeConnectRoomDto>> ListRooms(string folderSco)
    {
        var sessionParams = new NameValueCollection
        {
            {"sco-id", folderSco},
            {"filter-type", "meeting"}
        };

        if (_logOnly)
        {
            _logger.Information("ListRooms: folderSco={folderSco}, sessionParams={@sessionParams}", folderSco, sessionParams.ToDictionary());

            return new List<AdobeConnectRoomDto>() { new() { ScoId = "1", Name = "Test Room", UrlPath = "" } };
        }

        var response = await Request<MeetingLookup>("sco-expanded-contents", sessionParams);

        var returnData = new List<AdobeConnectRoomDto>();

        foreach (var room in response?.Rooms)
        {
            var roomResource = new AdobeConnectRoomDto
            {
                ScoId = room.ScoId,
                Name = room.Name,
                UrlPath = _settings.ServerUrl + room.UrlPath
            };

            returnData.Add(roomResource);
        }

        return returnData;
    }

    public async Task<string> GetCurrentSession(string scoId)
    {
        var sessionParams = new NameValueCollection
        {
            {"sco-id", scoId},
            {"sort-version", "desc"},
            {"filter-rows", "1"}
        };

        if (_logOnly)
        {
            _logger.Information("GetCurrentSession: scoId={scoId}, sessionParams={@sessionParams}", scoId, sessionParams.ToDictionary());

            return "1";
        }

        var response = await Request<SessionLookup>("report-meeting-sessions", sessionParams);

        return response?.Sessions.FirstOrDefault(s => s.DateEnded == null)?.AssetId;
    }
}
