﻿namespace Constellation.Infrastructure.ExternalServices.Canvas;

using Application.DTOs;
using Application.DTOs.Canvas;
using Application.Interfaces.Gateways;
using Core.Models.Attachments.DTOs;
using Core.Models.Canvas.Models;
using Microsoft.Extensions.Options;
using Models;
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

internal sealed class Gateway : ICanvasGateway
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
            _logger.Information("Gateway initialised in log only mode");

            return;
        }

        _url = configuration.Value.ApiEndpoint;
        _apiKey = configuration.Value.ApiKey;

        HttpClientHandler config = new() { CookieContainer = new CookieContainer() };

        IWebProxy proxy = WebRequest.DefaultWebProxy;
        config.UseProxy = true;
        config.Proxy = proxy;

        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        _client = new HttpClient(config);
    }

    private enum HttpVerb
    {
        Get,
        Post,
        Put,
        Delete
    }

    private async Task<HttpResponseMessage> RequestAsync(string path, HttpVerb action, object payload = null,
        CancellationToken cancellationToken = default)
    {
        Uri uri = path.StartsWith("http") ? new Uri(path) : new Uri($"{_url}/{path}");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        HttpResponseMessage response = action switch
        {
            HttpVerb.Get => await _client.GetAsync(uri, cancellationToken),
            HttpVerb.Post => await _client.PostAsJsonAsync(uri, payload, cancellationToken),
            HttpVerb.Put => await _client.PutAsJsonAsync(uri, payload, cancellationToken),
            HttpVerb.Delete => await _client.DeleteAsync(uri, cancellationToken),
            _ => new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest },
        };
        return response;
    }

    private async Task<int?> SearchForUserLogin(
        string userId,
        CancellationToken cancellationToken = default)
    {
        string path = $"users/sis_user_id:{userId}/logins";

        if (_logOnly)
        {
            _logger.Information("SearchForUserLogin: UserId={userId}, path={path}", userId, path);

            return 1;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<UserLoginResult> logins = JsonConvert.DeserializeObject<List<UserLoginResult>>(responseText);

        return logins.FirstOrDefault(login => login.SISId == userId)?.Id;
    }

    private async Task<int?> SearchForUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        string path = $"accounts/1/users?search_term={userId}";

        if (_logOnly)
        {
            _logger.Information("SearchForUser: UserId={userId}, path={path}", userId, path);

            return 1;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<UserResult> users = JsonConvert.DeserializeObject<List<UserResult>>(responseText);

        return users.FirstOrDefault(login => login.SISId == userId)?.Id;
    }

    private async Task<List<(int, string)>> SearchForCourseEnrolment(
        string userId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        List<(int, string)> data = new();

        string path = $"courses/sis_course_id:{courseId}/enrollments?sis_user_id={userId}";

        if (_logOnly)
        {
            _logger.Information("SearchForCourseEnrolment: UserId={userId}, CourseId={courseId}, path={path}", userId,
                courseId, path);

            return data;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<EnrolmentResult> enrolments = JsonConvert.DeserializeObject<List<EnrolmentResult>>(responseText);

        foreach (EnrolmentResult enrolment in enrolments)
            data.Add(new (enrolment.Id, enrolment.SectionId));

        return data;
    }

    private async Task<List<AssignmentResult>> SearchForCourseAssignment(
        string userId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"users/sis_user_id:{userId}/courses/sis_course_id:{courseId}/assignments";

        if (_logOnly)
        {
            _logger.Information("SearchForCourseAssignment: UserId={userId}, CourseId={courseId}, path={path}", userId,
                courseId, path);

            return new();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<AssignmentResult> assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

        return assignments;
    }

    public async Task<bool> UploadAssignmentSubmission(
        CanvasCourseCode courseId,
        int canvasAssignmentId,
        string studentId,
        AttachmentResponse file,
        CancellationToken cancellationToken = default)
    {
        string stepOnePath =
            $"courses/sis_course_id:{courseId}/assignments/{canvasAssignmentId}/submissions/sis_user_id:{studentId}/files";

        var stepOnePayload = new
        {
            name = file.FileName,
            size = file.FileData.Length,
            content_type = file.FileType,
            on_duplicate = "overwrite"
        };

        if (_logOnly)
        {
            _logger.Information(
                "UploadAssignmentSubmission: CourseId={courseId}, CanvasAssignmentId={canvasAssignmentId}, StudentId={studentId}, Attachment={@file}, stepOnePath={stepOnePath}, stepOnePayload={@stepOnePayload}",
                courseId, canvasAssignmentId, studentId, file, stepOnePath, stepOnePayload);

            return true;
        }

        HttpResponseMessage stepOneResponse = await RequestAsync(stepOnePath, HttpVerb.Post, stepOnePayload,
            cancellationToken: cancellationToken);
        if (!stepOneResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step one with response {@response}",
                stepOneResponse);
            return false;
        }

        string stepOneResponseText = await stepOneResponse.Content.ReadAsStringAsync(cancellationToken);
        FileUploadLocationResult fileUploadLocation =
            JsonConvert.DeserializeObject<FileUploadLocationResult>(stepOneResponseText);

        _logger.Information("CanvasGateway.UploadAssignmentSubmission: Succeeded on step one with response {@response}",
            fileUploadLocation);

        int fileId;

        // MultipartFormDataContent code taken from https://makolyte.com/csharp-how-to-send-a-file-with-httpclient/
        using (MultipartFormDataContent stepTwoContent = new())
        {
            stepTwoContent.Add(new StringContent(file.FileName), name: "filename");
            stepTwoContent.Add(new StringContent(file.FileType), name: "content_type");

            MemoryStream fileStream = new(file.FileData);
            StreamContent fileStreamContent = new(fileStream);
            fileStreamContent.Headers.ContentType = new(file.FileType);
            stepTwoContent.Add(fileStreamContent, name: "file", fileName: file.FileName);

            HttpResponseMessage stepTwoResponse = await _client.PostAsync(fileUploadLocation.UploadUrl, stepTwoContent,
                cancellationToken: cancellationToken);
            if (!stepTwoResponse.IsSuccessStatusCode)
            {
                _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step two with response {@response}",
                    stepTwoResponse);

                return false;
            }

            string stepTwoResponseText = await stepTwoResponse.Content.ReadAsStringAsync(cancellationToken);
            FileUploadConfirmationResult fileUploadConfirmation =
                JsonConvert.DeserializeObject<FileUploadConfirmationResult>(stepTwoResponseText);
            fileId = fileUploadConfirmation.Id;

            _logger.Information(
                "CanvasGateway.UploadAssignmentSubmission: Succeeded on step two with response {@response}",
                fileUploadConfirmation);
        }

        int? canvasUserId = await SearchForUser(studentId, cancellationToken);

        string stepThreePath = $"/courses/sis_course_id:{courseId}/assignments/{canvasAssignmentId}/submissions";
        var stepThreePayload = new
        {
            submission = new
            {
                submission_type = "online_upload", 
                file_ids = new[] { fileId }, 
                user_id = canvasUserId
            }
        };

        HttpResponseMessage stepThreeResponse =
            await RequestAsync(stepThreePath, HttpVerb.Post, stepThreePayload, cancellationToken);
        if (!stepThreeResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step three with response {@response}",
                stepThreeResponse);

            return false;
        }

        _logger.Information(
            "CanvasGateway.UploadAssignmentSubmission: Succeeded on step three with response {@response}",
            stepThreeResponse);

        return true;
    }

    public async Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/assignments";

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return new List<CanvasAssignmentDto>();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<AssignmentResult> assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

        List<CanvasAssignmentDto> returnData = new();

        assignments = assignments.Where(assignment =>
                assignment.IsPublished &&
                assignment.SubmissionTypes.Contains("online_upload"))
            .ToList();

        foreach (AssignmentResult assignment in assignments)
        {
            returnData.Add(new()
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

    public async Task<bool> CreateUser(
        string userId,
        string firstName,
        string lastName,
        string loginEmail,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        int? canvasUserId = await SearchForUser(userId, cancellationToken);

        if (_logOnly)
        {
            // The SearchForUser function will always return 1,
            // but we here want to return null instead to cover the whole method code.
            canvasUserId = null;
        }

        if (canvasUserId != null)
            return await ReactivateUser(userId, cancellationToken);

        // If not, create a new user
        string path = "accounts/1/users";

        var payload = new
        {
            user = new
            {
                name = $"{firstName} {lastName}", 
                short_name = $"{firstName} {lastName}"
            },
            pseudonym = new
            {
                unique_id = loginEmail, 
                authentication_provider_id = "google", 
                sis_user_id = userId
            },
            communications_channel = new
            {
                type = "email", 
                address = userEmail, 
                skip_confirmation = true
            }
        };

        if (_logOnly)
        {
            _logger.Information("CreateUser: args={@args}, payload={@payload}, path={path}",
                new
                {
                    UserId = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    LoginEmail = loginEmail,
                    UserEmail = userEmail
                }, payload, path);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EnrolToCourse(
        string userId,
        CanvasCourseCode courseId,
        CanvasPermissionLevel permissionLevel,
        CancellationToken cancellationToken = default)
    {
        int? canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId == null)
            return false;
        
        string path = $"courses/sis_course_id:{courseId}/enrollments";

        var payload = new
        {
            enrollment = new
            {
                user_id = canvasUserId.Value, 
                type = permissionLevel.Value, 
                enrollment_state = "active"
            }
        };

        if (_logOnly)
        {
            _logger.Information(
                "EnrolUser: UserId={userId}, CourseId={courseId}, PermissionLevel={permissionLevel}, path={path}, payload={@payload}",
                userId, courseId, permissionLevel, path, payload);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EnrolToSection(
        string userId,
        CanvasSectionCode sectionId,
        CanvasPermissionLevel permissionLevel,
        CancellationToken cancellationToken = default)
    {
        int? canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId == null)
            return false;

        bool sectionExists = await CheckSectionExists(sectionId, cancellationToken);

        if (!sectionExists)
        {
            bool sectionCreated = await CreateSection(sectionId, cancellationToken);

            if (!sectionCreated)
                return false;
        }

        string path = $"sections/sis_section_id:{sectionId}/enrollments";

        var payload = new
        {
            enrollment = new
            {
                user_id = canvasUserId.Value, 
                type = permissionLevel.Value, 
                enrollment_state = "active"
            }
        };

        if (_logOnly)
        {
            _logger.Information(
                "EnrolUser: UserId={userId}, SectionId={sectionId}, PermissionLevel={permissionLevel}, path={path}, payload={@payload}",
                userId, sectionId, permissionLevel, path, payload);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnenrolUser(
        string userId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        List<(int Id, string Section)> canvasEnrolments = await SearchForCourseEnrolment(userId, courseId, cancellationToken);

        if (canvasEnrolments.Count == 0)
            return false;

        foreach ((int Id, string Section) enrolment in canvasEnrolments)
        {
            bool unenrolSuccess = await UnenrolUser(enrolment.Id, courseId, cancellationToken);

            if (!unenrolSuccess)
                return false;
        }

        return true;
    }

    public async Task<bool> UnenrolUser(
        int enrollmentId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/enrollments/{enrollmentId}?task=deactivate";

        if (_logOnly)
        {
            _logger.Information("UnenrolUser: EnrollmentId={enrollmentId}, CourseId={courseId}, path={path}", enrollmentId, courseId, path);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReactivateUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        int? canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId == null)
            return false;

        string path = $"accounts/1/logins/{canvasLoginId}";

        var payload = new
        {
            login = new
            {
                workflow_state = "active"
            }
        };

        if (_logOnly)
        {
            _logger.Information("ReactivateUser: UserId={userId}, path={path}, payload={@payload}", userId, path,
                payload);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Put, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        int? canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId == null)
            return false;

        string path = $"accounts/1/logins/{canvasLoginId}";

        var payload = new
        {
            login = new
            {
                workflow_state = "suspended"
            }
        };

        if (_logOnly)
        {
            _logger.Information("DeactivateUser: UserId={userId}, path={path}, payload={@payload}", userId, path,
                payload);

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Put, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        int? canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId == null)
            return false;

        int? canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId == null)
            return false;

        // Change the users SIS_USER_ID to prepend the deletion year (making it unique)
        string path = $"accounts/1/logins/{canvasLoginId}";

        var payload = new
        {
            login = new
            {
                sis_user_id = $"{DateTime.Now.Year}-{userId}"
            }
        };

        HttpResponseMessage response;

        if (_logOnly)
        {
            _logger.Information("DeleteUser: UserId={userId}, path={path}, payload={@payload}", userId, path, payload);

            response = new(HttpStatusCode.NoContent);
        }
        else
        {
            response = await RequestAsync(path, HttpVerb.Put, payload, cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
            return false;

        path = $"accounts/1/users/{canvasUserId}";

        if (_logOnly)
        {
            _logger.Information("DeleteUser: UserId={userId}, path={path}", userId, path);

            return true;
        }

        response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<CourseListEntry>> GetAllCourses(
        string year,
        CancellationToken cancellationToken = default)
    {
        List<CourseListEntry> returnData = new();

        string path = $"accounts/1/courses?search_by=course&search_term={year}";

        bool nextPageExists = true;

        List<CourseListResult> courses = new();

        while (nextPageExists)
        {
            HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

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

        returnData.AddRange(courses.Where(entry => !string.IsNullOrWhiteSpace(entry.SISId))
            .Select(entry => new CourseListEntry(entry.Name, CanvasCourseCode.FromValue(entry.SISId))));

        return returnData;
    }

    public async Task<List<CourseEnrolmentEntry>> GetEnrolmentsForCourse(
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        List<CourseEnrolmentEntry> returnData = new();

        string path = $"courses/sis_course_id:{courseId}/enrollments";

        bool nextPageExists = true;

        List<EnrolmentListEntry> enrolments = new();

        while (nextPageExists)
        {
            HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

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
                enrolment.SISId.Length == 9 && enrolment.SISId.StartsWith("4") ? CourseEnrolmentEntry.UserType.Student
                : !string.IsNullOrWhiteSpace(enrolment.SISId) ? CourseEnrolmentEntry.UserType.Teacher
                : CourseEnrolmentEntry.UserType.Unknown;

            CourseEnrolmentEntry.EnrolmentRole role =
                enrolment.EnrollmentType switch
                {
                    "StudentEnrollment" => CourseEnrolmentEntry.EnrolmentRole.Student,
                    "TeacherEnrollment" => CourseEnrolmentEntry.EnrolmentRole.Teacher,
                    _ => CourseEnrolmentEntry.EnrolmentRole.Unknown
                };

            returnData.Add(new(
                enrolment.EnrollmentId,
                courseId,
                enrolment.SectionId is null ? CanvasSectionCode.Empty : CanvasSectionCode.FromValue(enrolment.SectionId),
                enrolment.SISId,
                userType,
                role));
        }

        return returnData;
    }

    public async Task<bool> AddUserToGroup(
        string userId,
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        int? canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId == null)
            return false;

        // Confirm group exists
        bool groupExists = await CheckGroupExists(groupId, cancellationToken);

        if (!groupExists)
        {
            bool categoryExists = await CheckGroupCategoryExists(groupId.ToString()[..^2], cancellationToken);

            if (!categoryExists)
            {
                bool categoryCreated = await CreateGroupCategory(groupId.ToString()[..^2], cancellationToken);

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
            _logger
                .ForContext(nameof(payload), payload, true)
                .ForContext(nameof(userId), userId)
                .ForContext(nameof(groupId), groupId)
                .ForContext(nameof(path), path)
                .Information("AddUserToGroup");

            return true;
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> CheckSectionExists(
        CanvasSectionCode sectionId,
        CancellationToken cancellationToken = default)
    {
        string path = $"sections/sis_section_id:{sectionId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> CreateSection(
        CanvasSectionCode sectionId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{sectionId.ToString()[..^2]}/sections";

        var payload = new
        {
            course_section = new {
                name = sectionId.ToString()[5..], 
                sis_section_id = sectionId.ToString()
            }
        };

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> CheckGroupExists(
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> CheckGroupCategoryExists(
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        string path = $"group_categories/sis_group_category_id:{categoryId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> CreateGroupCategory(
        string categoryId,
        CancellationToken cancellationToken = default)
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

    private async Task<bool> CreateGroup(
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        string path = $"group_categories/sis_group_category_id:{groupId.ToString()[..^2]}/groups";

        var payload = new
        {
            name = groupId.ToString()[5..], 
            join_level = "invitation_only", 
            sis_group_id = groupId.ToString()
        };

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>> GetGroupMembers(
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        string path = $"groups/sis_group_id:{groupId}/memberships";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return returnData;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<GroupMembershipListResult> groupMembers =
            JsonConvert.DeserializeObject<List<GroupMembershipListResult>>(responseText);

        foreach (GroupMembershipListResult member in groupMembers)
        {
            string userId = await GetUserData(member.CanvasUserId, cancellationToken);

            if (string.IsNullOrWhiteSpace(userId))
                continue;

            returnData.Add(userId);
        }

        return returnData;
    }

    private async Task<string> GetUserData(
        int canvasUserId,
        CancellationToken cancellationToken = default)
    {
        string path = $"users/{canvasUserId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return string.Empty;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        UserDetailsEntry userDetails = JsonConvert.DeserializeObject<UserDetailsEntry>(responseText);

        return userDetails is not null ? userDetails.UserId : string.Empty;
    }

    public async Task<bool> RemoveUserFromGroup(
        string userId,
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}/users/sis_user_id:{userId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode;
    }
}