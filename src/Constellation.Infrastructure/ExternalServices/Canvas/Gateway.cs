namespace Constellation.Infrastructure.ExternalServices.Canvas;

using Application.DTOs.Canvas;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.Canvas.Models;
using Core.Models.Attachments.DTOs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using static Constellation.Core.Errors.DomainErrors;

internal class Gateway : ICanvasGateway
{
    private readonly HttpClient _client;

    private readonly string _url;
    private readonly string _apiKey;
    private readonly ILogger _logger;

    private readonly bool _logOnly;

    public Gateway(
        IOptions<CanvasGatewayConfiguration> configuration,
        ILogger logger)
    {
        _logger = logger.ForContext<ICanvasGateway>();

        _logOnly = !configuration.Value.IsConfigured();

        if (_logOnly)
        {
            _logger.Information("Gateway initalised in log only mode");

            return;
        }

        _url = configuration.Value.ApiEndpoint;
        _apiKey = configuration.Value.ApiKey;

        HttpClientHandler config = new()
        {
            CookieContainer = new CookieContainer()
        };

        IWebProxy proxy = WebRequest.DefaultWebProxy;
        config.UseProxy = true;
        config.Proxy = proxy;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        _client = new HttpClient(config);
    }

    private enum HttpVerb
    {
        Get,
        Post,
        Put,
        Delete
    }

    private async Task<HttpResponseMessage> RequestAsync(string path, HttpVerb action, object payload = null, CancellationToken cancellationToken = default)
    {
        Uri uri = path.StartsWith("http") ? new Uri(path) : new Uri($"{_url}/{path}");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        HttpResponseMessage response = action switch
        {
            HttpVerb.Get => await _client.GetAsync(uri, cancellationToken),
            HttpVerb.Post => await _client.PostAsJsonAsync(uri, payload, cancellationToken),
            HttpVerb.Put => await _client.PutAsJsonAsync(uri, payload, cancellationToken),
            HttpVerb.Delete => await _client.DeleteAsync(uri, cancellationToken),
            _ => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            },
        };
        return response;
    }

    private async Task<int?> SearchForUserLogin(string UserId)
    {
        var path = $"users/sis_user_id:{UserId}/logins";

        if (_logOnly)
        {
            _logger.Information("SearchForUserLogin: UserId={userId}, path={path}", UserId, path);

            return 1;
        }

        var response = await RequestAsync(path, HttpVerb.Get);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseText = await response.Content.ReadAsStringAsync();

        var logins = JsonConvert.DeserializeObject<List<UserLoginResult>>(responseText);

        return logins.FirstOrDefault(login => login.SISId == UserId)?.Id;
    }

    private async Task<int?> SearchForUser(string UserId)
    {
        var path = $"accounts/1/users?search_term={UserId}";

        if (_logOnly)
        {
            _logger.Information("SearchForUser: UserId={userId}, path={path}", UserId, path);

            return 1;
        }

        var response = await RequestAsync(path, HttpVerb.Get);
        var responseText = await response.Content.ReadAsStringAsync();

        var users = JsonConvert.DeserializeObject<List<UserResult>>(responseText);

        return users.FirstOrDefault(login => login.SISId == UserId)?.Id;
    }

    private async Task<int?> SearchForCourseEnrolment(string UserId, string CourseId)
    {
        var path = $"courses/sis_course_id:{CourseId}/enrollments?sis_user_id={UserId}";

        if (_logOnly)
        {
            _logger.Information("SearchForCourseEnrolment: UserId={userId}, CourseId={courseId}, path={path}", UserId, CourseId, path);

            return 1;
        }

        var response = await RequestAsync(path, HttpVerb.Get);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseText = await response.Content.ReadAsStringAsync();

        var enrolments = JsonConvert.DeserializeObject<List<EnrolmentResult>>(responseText);

        return enrolments.FirstOrDefault(enrol => enrol.SISId == UserId)?.Id;
    }

    private async Task<List<AssignmentResult>> SearchForCourseAssignment(string UserId, string CourseId)
    {
        var path = $"users/sis_user_id:{UserId}/courses/sis_course_id:{CourseId}/assignments";

        if (_logOnly)
        {
            _logger.Information("SearchForCourseAssignment: UserId={userId}, CourseId={courseId}, path={path}", UserId, CourseId, path);

            return new List<AssignmentResult>();
        }


        var response = await RequestAsync(path, HttpVerb.Get);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseText = await response.Content.ReadAsStringAsync();

        var assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

        return assignments;
    }

    public async Task<bool> UploadAssignmentSubmission(string CourseId, int CanvasAssignmentId, string StudentId, AttachmentResponse file)
    {
        var stepOnePath = $"courses/sis_course_id:{CourseId}/assignments/{CanvasAssignmentId}/submissions/sis_user_id:{StudentId}/files";

        var stepOnePayload = new
        {
            name = file.FileName,
            size = file.FileData.Length,
            content_type = file.FileType,
            on_duplicate = "overwrite"
        };

        if (_logOnly)
        {
            _logger.Information("UploadAssignmentSubmission: CourseId={courseId}, CanvasAssignmentId={canvasAssignmentId}, StudentId={studentId}, Attachment={@file}, stepOnePath={stepOnePath}, stepOnePayload={@stepOnePayload}", CourseId, CanvasAssignmentId, StudentId, file, stepOnePath, stepOnePayload);

            return true;
        }

        var stepOneResponse = await RequestAsync(stepOnePath, HttpVerb.Post, stepOnePayload);
        if (!stepOneResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step one with response {@response}", stepOneResponse);
            return false;
        }

        var stepOneResponseText = await stepOneResponse.Content.ReadAsStringAsync();
        var fileUploadLocation = JsonConvert.DeserializeObject<FileUploadLocationResult>(stepOneResponseText);

        _logger.Information("CanvasGateway.UploadAssignmentSubmission: Succeeded on step one with response {@response}", fileUploadLocation);

        int fileId = 0;

        // MultipartFormDataContent code taken from https://makolyte.com/csharp-how-to-send-a-file-with-httpclient/
        using (var stepTwoContent = new MultipartFormDataContent())
        {
            stepTwoContent.Add(new StringContent(file.FileName), name: "filename");
            stepTwoContent.Add(new StringContent(file.FileType), name: "content_type");

            var fileStream = new MemoryStream(file.FileData);
            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.FileType);
            stepTwoContent.Add(fileStreamContent, name: "file", fileName: file.FileName);

            var stepTwoResponse = await _client.PostAsync(fileUploadLocation.UploadUrl, stepTwoContent);
            if (!stepTwoResponse.IsSuccessStatusCode)
            {
                _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step two with response {@response}", stepTwoResponse);

                return false;
            }

            var stepTwoResponseText = await stepTwoResponse.Content.ReadAsStringAsync();
            var fileUploadConfirmation = JsonConvert.DeserializeObject<FileUploadConfirmationResult>(stepTwoResponseText);
            fileId = fileUploadConfirmation.Id;

            _logger.Information("CanvasGateway.UploadAssignmentSubmission: Succeeded on step two with response {@response}", fileUploadConfirmation);
        }

        var canvasUserId = await SearchForUser(StudentId);

        var stepThreePath = $"/courses/sis_course_id:{CourseId}/assignments/{CanvasAssignmentId}/submissions";
        var stepThreePayload = new
        {
            submission = new
            {
                submission_type = "online_upload",
                file_ids = new[] { fileId },
                user_id = canvasUserId
            }
        };

        var stepThreeResponse = await RequestAsync(stepThreePath, HttpVerb.Post, stepThreePayload);
        if (!stepThreeResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step three with response {@response}", stepThreeResponse);

            return false;
        }

        _logger.Information("CanvasGateway.UploadAssignmentSubmission: Succeeded on step three with response {@response}", stepThreeResponse);

        return true;
    }

    public async Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(string CourseId)
    {
        var path = $"courses/sis_course_id:{CourseId}/assignments";

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", CourseId, path);

            return new List<CanvasAssignmentDto>();
        }

        var response = await RequestAsync(path, HttpVerb.Get);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseText = await response.Content.ReadAsStringAsync();

        var assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

        var returnData = new List<CanvasAssignmentDto>();

        foreach (var assignment in assignments.Where(a => a.IsPublished && a.SubmissionTypes.Contains("online_upload")))
        {
            returnData.Add(new CanvasAssignmentDto
            {
                CanvasId = assignment.Id,
                Name = assignment.Name,
                DueDate = assignment.DueDate ?? DateTime.Today,
                LockDate = assignment.LockDate,
                UnlockDate = assignment.UnlockDate,
                AllowedAttempts = assignment.AllowedAttempts
            });
        }

        return returnData;
    }

    public async Task<bool> CreateUser(string UserId, string FirstName, string LastName, string LoginEmail, string UserEmail)
    {
        var CanvasUserId = await SearchForUser(UserId);

        if (_logOnly)
        {
            // The SearchForUser function will always return 1,
            // but we here want to return null instead to cover the create code.
            CanvasUserId = null;
        }

        if (CanvasUserId != null)
            return await ReactivateUser(UserId);

        // If not, create a new user
        var path = "accounts/1/users";

        var payload = new
        {
            user = new
            {
                name = $"{FirstName} {LastName}",
                short_name = $"{FirstName} {LastName}"
            },
            pseudonym = new
            {
                unique_id = LoginEmail,
                authentication_provider_id = "google",
                sis_user_id = UserId
            },
            communications_channel = new
            {
                type = "email",
                address = UserEmail,
                skip_confirmation = true
            }
        };

        if (_logOnly)
        {
            _logger.Information("CreateUser: args={@args}, payload={@payload}, path={path}", new { UserId, FirstName, LastName, LoginEmail, UserEmail }, payload, path);

            return true;
        }

        var response = await RequestAsync(path, HttpVerb.Post, payload);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }

    public async Task<bool> EnrolUser(string UserId, string CourseId, string PermissionLevel)
    {
        var CanvasUserId = await SearchForUser(UserId);

        if (CanvasUserId == null)
            return false;

        var path = $"courses/sis_course_id:{CourseId}/enrollments";

        var payload = new
        {
            enrollment = new
            {
                user_id = CanvasUserId,
                type = $"{PermissionLevel}Enrollment",
                enrollment_state = "active"
            }
        };

        if (_logOnly)
        {
            _logger.Information("EnrolUser: UserId={userId}, CourseId={courseId}, PermissionLevel={permissionLevel}, path={path}, payload={@payload}", UserId, CourseId, PermissionLevel, path, payload);

            return true;
        }

        var response = await RequestAsync(path, HttpVerb.Post, payload);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }

    public async Task<bool> UnenrolUser(string UserId, string CourseId)
    {
        var CanvasEnrolmentId = await SearchForCourseEnrolment(UserId, CourseId);

        if (CanvasEnrolmentId == null)
            return false;

        var path = $"courses/sis_course_id:{CourseId}/enrollments/{CanvasEnrolmentId}?task=deactivate";

        if (_logOnly)
        {
            _logger.Information("UnenrolUser: UserId={userId}, CourseId={courseId}, path={path}", UserId, CourseId, path);

            return true;
        }

        var response = await RequestAsync(path, HttpVerb.Delete);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }


    public async Task<bool> ReactivateUser(string UserId)
    {
        var CanvasLoginId = await SearchForUserLogin(UserId);

        if (CanvasLoginId == null)
            return false;

        var path = $"accounts/1/logins/{CanvasLoginId}";

        var payload = new
        {
            login = new
            {
                workflow_state = "active"
            }
        };

        if (_logOnly)
        {
            _logger.Information("ReactivateUser: UserId={userId}, path={path}, payload={@payload}", UserId, path, payload);

            return true;
        }

        var response = await RequestAsync(path, HttpVerb.Put, payload);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }

    public async Task<bool> DeactivateUser(string UserId)
    {
        var CanvasLoginId = await SearchForUserLogin(UserId);

        if (CanvasLoginId == null)
            return false;

        var path = $"accounts/1/logins/{CanvasLoginId}";

        var payload = new
        {
            login = new
            {
                workflow_state = "suspended"
            }
        };

        if (_logOnly)
        {
            _logger.Information("DeactivateUser: UserId={userId}, path={path}, payload={@payload}", UserId, path, payload);

            return true;
        }

        var response = await RequestAsync(path, HttpVerb.Put, payload);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }

    public async Task<bool> DeleteUser(string UserId)
    {
        var CanvasLoginId = await SearchForUserLogin(UserId);

        if (CanvasLoginId == null)
            return false;

        var CanvasUserId = await SearchForUser(UserId);

        if (CanvasUserId == null)
            return false;

        // Change the users SIS_USER_ID to prepend the deletion year (making it unique)
        var path = $"accounts/1/logins/{CanvasLoginId}";

        var payload = new
        {
            login = new
            {
                sis_user_id = $"{DateTime.Now.Year}-{UserId}"
            }
        };

        HttpResponseMessage response;

        if (_logOnly)
        {
            _logger.Information("DeleteUser: UserId={userId}, path={path}, payload={@payload}", UserId, path, payload);

            response = new HttpResponseMessage(HttpStatusCode.NoContent);
        }
        else
        {
            response = await RequestAsync(path, HttpVerb.Put, payload);
        }

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        path = $"accounts/1/users/{CanvasUserId}";

        if (_logOnly)
        {
            _logger.Information("DeleteUser: UserId={userId}, path={path}", UserId, path);

            return true;
        }

        response = await RequestAsync(path, HttpVerb.Delete);

        if (response.IsSuccessStatusCode)
            return true;

        return false;
    }

    public async Task<List<CourseListEntry>> GetAllCourses(string year, CancellationToken cancellationToken = default)
    {
        List<CourseListEntry> returnData = new();

        string path = $"accounts/1/courses?search_by=course&search_term={year}";

        bool nextPageExists = true;

        List<CourseListResult> courses = new();

        while (nextPageExists)
        {
            HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get);

            if (!response.IsSuccessStatusCode)
                return returnData;

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            courses.AddRange(JsonConvert.DeserializeObject<List<CourseListResult>>(responseText));

            bool responseHeaders = response.Headers.TryGetValues("link", out IEnumerable<string> linkHeaders);

            if (!responseHeaders)
                nextPageExists = false;
            
            string[] links = linkHeaders!.First().Split(',');
            string nextLinkHeader = links.FirstOrDefault(entry => entry.Contains(@"rel=""next"""));

            if (nextLinkHeader is null)
            {
                nextPageExists = false;
            }
            else
            {
                string[] parts = nextLinkHeader.Split(";");
                path = parts[0].TrimStart('<').TrimEnd('>');
            }
        }

        returnData.AddRange(courses.Where(entry => !string.IsNullOrWhiteSpace(entry.SISId)).Select(entry => new CourseListEntry(entry.Name, entry.SISId)));

        return returnData;
    }

    public async Task<List<CourseEnrolmentEntry>> GetEnrolmentsForCourse(string courseId, CancellationToken cancellationToken = default)
    {
        List<CourseEnrolmentEntry> returnData = new();

        string path = $"courses/sis_course_id:{courseId}/enrollments";

        bool nextPageExists = true;

        List<EnrolmentListEntry> enrolments = new();

        while (nextPageExists)
        {
            HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get);

            if (!response.IsSuccessStatusCode)
                return null;

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            enrolments.AddRange(JsonConvert.DeserializeObject<List<EnrolmentListEntry>>(responseText));

            bool responseHeaders = response.Headers.TryGetValues("link", out IEnumerable<string> linkHeaders);

            if (!responseHeaders)
                nextPageExists = false;

            string[] links = linkHeaders!.First().Split(',');
            string nextLinkHeader = links.FirstOrDefault(entry => entry.Contains(@"rel=""next"""));

            if (nextLinkHeader is null)
            {
                nextPageExists = false;
            }
            else
            {
                string[] parts = nextLinkHeader.Split(";");
                path = parts[0].TrimStart('<').TrimEnd('>');
            }
        }

        foreach (EnrolmentListEntry enrolment in enrolments.Where(entry => entry.EnrollmentState == "active" && entry.EnrollmentType != "StudentViewEnrollment"))
        {
            CourseEnrolmentEntry.UserType userType =
                string.IsNullOrWhiteSpace(enrolment.SISId) ? CourseEnrolmentEntry.UserType.Unknown
                : enrolment.SISId.Length == 9 && enrolment.SISId.StartsWith("4") ? CourseEnrolmentEntry.UserType.Student
                : !string.IsNullOrWhiteSpace(enrolment.SISId) ? CourseEnrolmentEntry.UserType.Teacher
                : CourseEnrolmentEntry.UserType.Unknown;

            CourseEnrolmentEntry.EnrolmentRole role =
                enrolment.EnrollmentType == "StudentEnrollment" ? CourseEnrolmentEntry.EnrolmentRole.Student
                : enrolment.EnrollmentType == "TeacherEnrollment" ? CourseEnrolmentEntry.EnrolmentRole.Teacher
                : CourseEnrolmentEntry.EnrolmentRole.Unknown;

            returnData.Add(new(
                courseId,
                enrolment.SISId,
                userType,
                role));
        }

        return returnData;
    }

    public async Task<bool> AddUserToGroup(string userId, string groupId, CancellationToken cancellationToken = default)
    {
        int? canvasUserId = await SearchForUser(userId);

        if (canvasUserId == null)
            return false;

        // Confirm group exists
        bool groupExists = await CheckGroupExists(groupId, cancellationToken);

        if (!groupExists)
        {
            bool categoryExists = await CheckGroupCategoryExists(groupId[..^2], cancellationToken);

            if (!categoryExists)
            {
                bool categoryCreated = await CreateGroupCategory(groupId[..^2], cancellationToken);

                if (!categoryCreated)
                    return false;
            }

            bool groupCreated = await CreateGroup(groupId, cancellationToken);

            if (!groupCreated) 
                return false;
        }
        
        string path = $"groups/sis_group_id:{groupId}/memberships";

        var payload = new
        {
            user_id = canvasUserId
        };

        if (_logOnly)
        {
            _logger.Information("EnrolUser: UserId={userId}, CourseId={courseId}, PermissionLevel={permissionLevel}, path={path}, payload={@payload}", userId, groupId, path, payload);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CheckGroupExists(string groupId, CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CheckGroupCategoryExists(string categoryId, CancellationToken cancellationToken = default)
    {
        string path = $"group_categories/sis_group_cateogry_id:{categoryId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateGroupCategory(string categoryId, CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{categoryId}/group_categories";
        
        var payload = new
        {
            name = categoryId[..4],
            sis_group_category_id = categoryId
        };
        
        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateGroup(string groupId, CancellationToken cancellationToken = default)
    {
        string path = $"group_categories/sis_group_category_id:{groupId[..^2]}/groups";

        var payload = new
        {
            name = groupId[5..],
            join_level = "invitation_only",
            sis_group_id = groupId
        };

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>> GetGroupMembers(string groupId, CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        string path = $"groups/sis_group_id:{groupId}/memberships";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return returnData;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<GroupMembershipListResult> groupMembers = JsonConvert.DeserializeObject<List<GroupMembershipListResult>>(responseText);

        foreach (GroupMembershipListResult member in groupMembers)
        {
            string userId = await GetUserData(member.CanvasUserId, cancellationToken);

            if (string.IsNullOrWhiteSpace(userId))
                continue;

            returnData.Add(userId);
        }

        return returnData;
    }

    public async Task<string> GetUserData(int canvasUserId, CancellationToken cancellationToken = default)
    {
        string path = $"users/{canvasUserId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return string.Empty;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        UserDetailsEntry userDetails = JsonConvert.DeserializeObject<UserDetailsEntry>(responseText);

        return userDetails is not null ? userDetails.UserId : string.Empty;
    }

    public async Task<bool> RemoveUserFromGroup(string userId, string groupId, CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}/users/sis_user_id:{userId}";
        
        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
