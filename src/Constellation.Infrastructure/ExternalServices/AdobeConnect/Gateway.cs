using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Constellation.Infrastructure.ExternalServices.AdobeConnect
{
    public class Gateway : IAdobeConnectGateway, IScopedService
    {
        private readonly HttpClient _client;
        private readonly IAdobeConnectGatewayConfiguration _settings;
        private DateTime? _loginTime;
        private bool _loginInProgress;

        public Gateway(IAdobeConnectGatewayConfiguration settings)
        {
            _settings = settings;

            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.Url)
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
            var loginResponse = await _client.GetAsync(loginQuery);

            var cookie = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookie == null)
                throw new Exception("Could not log in to Adobe Connect");

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
            if (response == null || response.Status.Code != "ok")
                return false;

            return true;
        }

        private async Task<string> FindFolder(string year, string grade)
        {
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

            var response = await Request<RoomCreation>("sco-update", actionParams);

            if (response.Status.Code.ToLower() != "ok")
            {
                return null;
            }

            // TODO: Check for successful response here before continuing:

            //  TYPE: date
            //  SUBCODE: range
            //  FIELD: date-end (value: 2021-12-16)

            await AddDefaultRoomPermissions(response.Sco.ScoId, faculty);

            var newRoom = new AdobeConnectRoomDto
            {
                ScoId = response.Sco.ScoId,
                Name = response.Sco.Name,
                UrlPath = _settings.Url + response.Sco.UrlPath
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

            var response = await Request<MeetingLookup>("sco-expanded-contents", sessionParams);

            var returnData = new List<AdobeConnectRoomDto>();

            foreach (var room in response?.Rooms)
            {
                var roomResource = new AdobeConnectRoomDto
                {
                    ScoId = room.ScoId,
                    Name = room.Name,
                    UrlPath = _settings.Url + room.UrlPath
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

            var response = await Request<SessionLookup>("report-meeting-sessions", sessionParams);

            return response?.Sessions.FirstOrDefault(s => s.DateEnded == null)?.AssetId;
        }

        [XmlRoot("results")]
        public class ResponseDto
        {
            [XmlElement("status")]
            public Status Status { get; set; }
        }

        public class Status
        {
            [XmlAttribute("code")]
            public string Code { get; set; }
            [XmlElement("invalid")]
            public StatusInvalid ErrorCode { get; set; }
        }

        public class StatusInvalid
        {
            [XmlAttribute("field")]
            public string Field { get; set; }
            [XmlAttribute("type")]
            public string Type { get; set; }
            [XmlAttribute("subcode")]
            public string Subcode { get; set; }
        }

        public class UserLookup : ResponseDto
        {
            [XmlArray("principal-list"), XmlArrayItem("principal")]
            public List<Principal> PrincipalList { get; set; }
        }

        public class Principal
        {
            [XmlAttribute("principal-id")]
            public string PrincipalId { get; set; }
            [XmlElement("name")]
            public string Name { get; set; }
            [XmlElement("login")]
            public string Login { get; set; }
            [XmlElement("email")]
            public string Email { get; set; }
        }

        public class SessionLookup : ResponseDto
        {
            [XmlArray("report-meeting-sessions"), XmlArrayItem("row")]
            public List<MeetingSession> Sessions { get; set; }
        }

        public class MeetingSession
        {
            [XmlAttribute("sco-id")]
            public string ScoId { get; set; }
            [XmlAttribute("asset-id")]
            public string AssetId { get; set; }
            [XmlAttribute("num-participants")]
            public string ParticipantCount { get; set; }
            [XmlElement("date-created")]
            public DateTime? DateStarted { get; set; }
            [XmlElement("date-end")]
            public DateTime? DateEnded { get; set; }
        }

        public class AttendanceLookup : ResponseDto
        {
            [XmlArray("report-meeting-attendance"), XmlArrayItem("row")]
            public List<UserAttendance> Users { get; set; }
        }

        public class UserAttendance
        {
            [XmlAttribute("principal-id")]
            public string PrincipalId { get; set; }
            [XmlElement("login")]
            public string Login { get; set; }
            [XmlElement("session-name")]
            public string Name { get; set; }
            [XmlElement("date-created")]
            public DateTime? DateEntered { get; set; }
            [XmlElement("date-end")]
            public DateTime? DateLeft { get; set; }
        }

        public class FolderLookup : ResponseDto
        {
            [XmlArray("scos"), XmlArrayItem("sco")]
            public List<Folder> Scos { get; set; }
        }

        public class Folder
        {
            [XmlAttribute("sco-id")]
            public string ScoId { get; set; }
            [XmlElement("name")]
            public string Name { get; set; }
            [XmlElement("url-path")]
            public string UrlPath { get; set; }
        }

        public class RoomCreation : ResponseDto
        {
            [XmlElement("sco")]
            public RoomSco Sco { get; set; }
        }

        public class RoomSco
        {
            [XmlAttribute("sco-id")]
            public string ScoId { get; set; }
            [XmlElement("name")]
            public string Name { get; set; }
            [XmlElement("url-path")]
            public string UrlPath { get; set; }
        }

        public class MeetingLookup : ResponseDto
        {
            [XmlArray("expanded-scos"), XmlArrayItem("sco")]
            public List<MeetingRoom> Rooms { get; set; }
        }

        public class MeetingRoom
        {
            [XmlAttribute("sco-id")]
            public string ScoId { get; set; }
            [XmlElement("name")]
            public string Name { get; set; }
            [XmlElement("url-path")]
            public string UrlPath { get; set; }
        }
    }
}
