#nullable enable
namespace Constellation.Infrastructure.ExternalServices.Canvas;

using Application.DTOs;
using Application.DTOs.Canvas;
using Application.Interfaces.Gateways;
using Core.Models.Attachments.DTOs;
using Core.Models.Canvas.Models;
using Core.Shared;
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

    private async Task<List<T>> RequestAsync<T>(
        string path, 
        CancellationToken cancellationToken = default) 
        where T : class
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        List<T> completeResponse = new List<T>();

        bool nextPageExists = true;

        while (nextPageExists)
        {
            Uri uri = path.StartsWith("http") ? new Uri(path) : new Uri($"{_url}/{path}");
            
            HttpResponseMessage response = await _client.GetAsync(uri, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return completeResponse;

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            completeResponse.AddRange(JsonConvert.DeserializeObject<List<T>>(responseText));

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

        return completeResponse;
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

    private async Task<Result<int>> SearchForUserLogin(
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
            return Result.Failure<int>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<UserLoginResult>? logins = JsonConvert.DeserializeObject<List<UserLoginResult>>(responseText);

        if (logins is null)
            return Result.Failure<int>(CanvasGatewayErrors.InvalidData);

        UserLoginResult? login = logins.FirstOrDefault(login => login.SISId == userId);

        if (login is null)
            return Result.Failure<int>(CanvasGatewayErrors.UserLoginNotFound(userId));

        return login.Id;
    }

    private async Task<Result<int>> SearchForUser(
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
            return Result.Failure<int>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<UserResult> users = JsonConvert.DeserializeObject<List<UserResult>>(responseText);

        UserResult user = users.FirstOrDefault(login => login.SISId == userId);

        if (user is null)
            return Result.Failure<int>(CanvasGatewayErrors.UserNotFound(userId));

        return user.Id;
    }

    private async Task<Result<List<(int, string)>>> SearchForCourseEnrolment(
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
            return Result.Failure<List<(int, string)>>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<EnrolmentResult>? enrolments = JsonConvert.DeserializeObject<List<EnrolmentResult>>(responseText);

        if (enrolments is null)
            return Result.Failure<List<(int, string)>>(CanvasGatewayErrors.InvalidData);

        foreach (EnrolmentResult enrolment in enrolments)
            data.Add(new (enrolment.Id, enrolment.SectionId));

        return data;
    }

    private async Task<Result<List<AssignmentResult>>> SearchForCourseAssignment(
        string userId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"users/sis_user_id:{userId}/courses/sis_course_id:{courseId}/assignments";

        if (_logOnly)
        {
            _logger.Information("SearchForCourseAssignment: UserId={userId}, CourseId={courseId}, path={path}", userId,
                courseId, path);

            return new List<AssignmentResult>();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure<List<AssignmentResult>>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<AssignmentResult>? assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

        if (assignments is null)
            return Result.Failure<List<AssignmentResult>>(CanvasGatewayErrors.InvalidData);

        return assignments;
    }

    public async Task<Result> UploadAssignmentSubmission(
        CanvasCourseCode courseId,
        int canvasAssignmentId,
        string studentReferenceNumber,
        AttachmentResponse file,
        CancellationToken cancellationToken = default)
    {
        string stepOnePath =
            $"courses/sis_course_id:{courseId}/assignments/{canvasAssignmentId}/submissions/sis_user_id:{studentReferenceNumber}/files";

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
                courseId, canvasAssignmentId, studentReferenceNumber, file, stepOnePath, stepOnePayload);

            return Result.Success();
        }

        HttpResponseMessage stepOneResponse = await RequestAsync(stepOnePath, HttpVerb.Post, stepOnePayload, cancellationToken: cancellationToken);
        
        if (!stepOneResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step one with response {@response}",
                stepOneResponse);
        
            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
        }

        string stepOneResponseText = await stepOneResponse.Content.ReadAsStringAsync(cancellationToken);
        FileUploadLocationResult? fileUploadLocation = JsonConvert.DeserializeObject<FileUploadLocationResult>(stepOneResponseText);

        if (fileUploadLocation is null)
            return Result.Failure(CanvasGatewayErrors.InvalidData);

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

            HttpResponseMessage stepTwoResponse = await _client.PostAsync(fileUploadLocation.UploadUrl, stepTwoContent, cancellationToken);
            
            if (!stepTwoResponse.IsSuccessStatusCode)
            {
                _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step two with response {@response}",
                    stepTwoResponse);

                return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
            }

            string stepTwoResponseText = await stepTwoResponse.Content.ReadAsStringAsync(cancellationToken);
            FileUploadConfirmationResult? fileUploadConfirmation = JsonConvert.DeserializeObject<FileUploadConfirmationResult>(stepTwoResponseText);

            if (fileUploadConfirmation is null)
                return Result.Failure(CanvasGatewayErrors.InvalidData);

            fileId = fileUploadConfirmation.Id;

            _logger.Information(
                "CanvasGateway.UploadAssignmentSubmission: Succeeded on step two with response {@response}",
                fileUploadConfirmation);
        }

        Result<int> canvasUserId = await SearchForUser(studentReferenceNumber, cancellationToken);

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

        HttpResponseMessage stepThreeResponse = await RequestAsync(stepThreePath, HttpVerb.Post, stepThreePayload, cancellationToken);

        if (!stepThreeResponse.IsSuccessStatusCode)
        {
            _logger.Error("CanvasGateway.UploadAssignmentSubmission: Failed on step three with response {@response}", stepThreeResponse);

            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
        }

        _logger.Information("CanvasGateway.UploadAssignmentSubmission: Succeeded on step three with response {@response}", stepThreeResponse);

        return Result.Success();
    }

    public async Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/assignments";
        
        List<CanvasAssignmentDto> returnData = new();
        
        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return new List<CanvasAssignmentDto>();
        }
        
        List<AssignmentResult> assignments = await RequestAsync<AssignmentResult>(path, cancellationToken: cancellationToken);
        
        assignments = assignments
            .Where(assignment =>
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

    public async Task<List<CanvasAssignmentDto>> GetAllUploadCourseAssignments(
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/assignments";

        List<CanvasAssignmentDto> returnData = new();

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return new List<CanvasAssignmentDto>();
        }

        List<AssignmentResult> assignments = await RequestAsync<AssignmentResult>(path, cancellationToken: cancellationToken);

        assignments = assignments
            .Where(assignment =>
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

    public async Task<List<CanvasAssignmentDto>> GetAllRubricCourseAssignments(
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/assignments";

        List<CanvasAssignmentDto> returnData = new();

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return new List<CanvasAssignmentDto>();
        }

        List<AssignmentResult> assignments = await RequestAsync<AssignmentResult>(path, cancellationToken: cancellationToken);

        assignments = assignments
            .Where(assignment =>
                assignment.IsPublished &&
                assignment.Rubric is not null)
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

    public async Task<Result<RubricEntry>> GetCourseAssignmentDetails(
        CanvasCourseCode courseId,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/assignments/{assignmentId}";

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return Result.Failure<RubricEntry>(CanvasGatewayErrors.FailureResponseCode);
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            return Result.Failure<RubricEntry>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        AssignmentSettingsResult? assessmentSettings = JsonConvert.DeserializeObject<AssignmentSettingsResult>(responseText);

        if (assessmentSettings is null)
            return Result.Failure<RubricEntry>(CanvasGatewayErrors.InvalidData);

        List<RubricEntry.RubricCriterion> criteria = new();

        if (assessmentSettings.Rubric is null)
            return Result.Failure<RubricEntry>(CanvasGatewayErrors.RubricNotIncluded);

        foreach (AssignmentSettingsResult.RubricItem criterion in assessmentSettings.Rubric)
        {
            List<RubricEntry.RubricCriterionRating> ratings = new();

            foreach (AssignmentSettingsResult.RubricItem rating in criterion.Ratings)
            {
                ratings.Add(new(
                    rating.Id,
                    rating.Points,
                    rating.Description,
                    rating.LongDescription));
            }

            criteria.Add(new(
                criterion.Id,
                criterion.Points,
                criterion.Description,
                criterion.LongDescription,
                ratings));
        }

        return new RubricEntry(
            assessmentSettings.Settings.Id,
            assessmentSettings.Settings.Title,
            assessmentSettings.Settings.PointsPossible,
            criteria);
    }

    public async Task<List<AssignmentResultEntry>> GetCourseAssignmentSubmissions(
        CanvasCourseCode courseId,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        List<AssignmentResultEntry> results = new();
        
        string path = $"courses/sis_course_id:{courseId}/assignments/{assignmentId}/submissions?include[]=rubric_assessment&include[]=submission_comments";

        if (_logOnly)
        {
            _logger.Information("GetAllCourseAssignments: CourseId={courseId}, path={path}", courseId, path);

            return results;
        }

        List<AssignmentSubmission> submissions = await RequestAsync<AssignmentSubmission>(path, cancellationToken: cancellationToken);
        
        foreach (AssignmentSubmission submission in submissions)
        {
            List<AssignmentResultEntry.AssignmentRubricResult> marks = new();

            foreach (KeyValuePair<string, AssignmentSubmission.RubricValue> mark in submission.RubricAssessment)
            {
                marks.Add(new(
                    mark.Key,
                    mark.Value.RatingId,
                    mark.Value.Comments,
                    mark.Value.Points));
            }

            List<AssignmentResultEntry.AssignmentComment> comments = new();

            foreach (AssignmentSubmission.SubmissionComments comment in submission.Comments)
            {
                comments.Add(new(
                    comment.Author,
                    comment.CreatedAt,
                    comment.Comment));
            }

            results.Add(new(
                submission.AssignmentId,
                courseId,
                submission.UserId,
                marks,
                comments,
                submission.Mark,
                submission.Grade));
        }

        return results;
    } 

    public async Task<Result> CreateUser(
        string userId,
        string firstName,
        string lastName,
        string loginEmail,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (_logOnly)
        {
            // The SearchForUser function will always return 1,
            // but we here want to return null instead to cover the whole method code.
            canvasUserId = Result.Failure<int>(CanvasGatewayErrors.FailureResponseCode);
        }

        if (canvasUserId.IsFailure)
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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode 
            ? Result.Success() 
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> UpdateUserEmail(
        string userId,
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        _logger
            .ForContext(nameof(userId), userId)
            .ForContext(nameof(emailAddress), emailAddress)
            .Information("Requested to update user email address");

        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId.IsFailure)
        {
            _logger
                .ForContext(nameof(userId), userId)
                .ForContext(nameof(emailAddress), emailAddress)
                .ForContext(nameof(Error), canvasUserId.Error, true)
                .Warning("Failed to find existing user at Canvas");

            return Result.Failure(canvasUserId.Error);
        }

        // Update communication email address
        string path = $"accounts/1/users/{canvasUserId.Value}";

        var commsPayload = new
        {
            user = new
            {
                email = emailAddress
            }
        };

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Put, commsPayload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger
                .ForContext(nameof(userId), userId)
                .ForContext(nameof(emailAddress), emailAddress)
                .Warning("Failed to update communication email field of existing user at Canvas");

            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
        }

        // Update login email address
        Result<int> canvasUserLogin = await SearchForUserLogin(userId, cancellationToken);

        if (canvasUserLogin.IsFailure)
        {
            _logger
                .ForContext(nameof(userId), userId)
                .ForContext(nameof(emailAddress), emailAddress)
                .Warning("Failed to find logins for existing user at Canvas");

            return Result.Failure(canvasUserLogin.Error);
        }

        path = $"accounts/1/logins/{canvasUserLogin.Value}";

        var loginPayload = new
        {
            login = new
            {
                unique_id = emailAddress
            }
        };

        response = await RequestAsync(path, HttpVerb.Put, loginPayload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger
                .ForContext(nameof(userId), userId)
                .ForContext(nameof(emailAddress), emailAddress)
                .Error("Failed to update login email field of existing user at Canvas");

            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
        }

        return Result.Success();
    }

    public async Task<Result> EnrolToCourse(
        string userId,
        CanvasCourseCode courseId,
        CanvasPermissionLevel permissionLevel,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId.IsFailure)
            return canvasUserId;
        
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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> EnrolToSection(
        string userId,
        CanvasSectionCode sectionId,
        CanvasPermissionLevel permissionLevel,
        CancellationToken cancellationToken = default)
    {
        if (sectionId == CanvasSectionCode.Empty)
        {
            // Section is invalid
            _logger
                .ForContext(nameof(CanvasSectionCode), sectionId, true)
                .ForContext(nameof(userId), userId, true)
                .Warning("Failed to enrol user to section");

            return Result.Failure(CanvasGatewayErrors.InvalidSectionCode(sectionId));
        }

        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId.IsFailure)
            return canvasUserId;

        Result sectionExists = await CheckSectionExists(sectionId, cancellationToken);

        if (sectionExists.IsFailure)
        {
            Result sectionCreated = await CreateSection(sectionId, cancellationToken);

            if (sectionCreated.IsFailure)
                return sectionCreated;
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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> UnenrolUser(
        string userId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        Result<List<(int, string)>> canvasEnrolments = await SearchForCourseEnrolment(userId, courseId, cancellationToken);

        if (canvasEnrolments.IsFailure)
            return canvasEnrolments;

        if (canvasEnrolments.Value.Count == 0)
            return Result.Failure(CanvasGatewayErrors.UserNotFoundInCourse(userId, courseId));

        foreach ((int Id, string Section) enrolment in canvasEnrolments.Value)
        {
            Result unenrolSuccess = await UnenrolUser(enrolment.Id, courseId, cancellationToken);

            if (unenrolSuccess.IsFailure)
                return unenrolSuccess;
        }

        return Result.Success();
    }

    public async Task<Result> UnenrolUser(
        int enrollmentId,
        CanvasCourseCode courseId,
        CancellationToken cancellationToken = default)
    {
        string path = $"courses/sis_course_id:{courseId}/enrollments/{enrollmentId}?task=deactivate";

        if (_logOnly)
        {
            _logger.Information("UnenrolUser: EnrollmentId={enrollmentId}, CourseId={courseId}, path={path}", enrollmentId, courseId, path);

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> ReactivateUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId.IsFailure)
            return Result.Failure(CanvasGatewayErrors.UserNotFound(userId));

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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Put, payload, cancellationToken);

        if (response.IsSuccessStatusCode)
            return Result.Success();
        else
            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> DeactivateUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId.IsFailure)
            return canvasLoginId;

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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Put, payload, cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result> DeleteUser(
        string userId,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasLoginId = await SearchForUserLogin(userId, cancellationToken);

        if (canvasLoginId.IsFailure)
            return canvasLoginId;

        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId.IsFailure)
            return canvasUserId;

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
            return Result.Failure(CanvasGatewayErrors.FailureResponseCode);

        path = $"accounts/1/users/{canvasUserId}";

        if (_logOnly)
        {
            _logger.Information("DeleteUser: UserId={userId}, path={path}", userId, path);

            return Result.Success();
        }

        response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<List<CourseListEntry>> GetAllCourses(
        string year,
        CancellationToken cancellationToken = default)
    {
        List<CourseListEntry> returnData = new();

        string path = $"accounts/1/courses?search_by=course&search_term={year[..^1]}&ends_after={year}-01-01";

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
                enrolment.User.CanvasUserId,
                userType,
                role));
        }

        return returnData;
    }

    public async Task<Result> AddUserToGroup(
        string userId,
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        Result<int> canvasUserId = await SearchForUser(userId, cancellationToken);

        if (canvasUserId.IsFailure)
            return canvasUserId;

        // Confirm group exists
        Result groupExists = await CheckGroupExists(groupId, cancellationToken);

        if (groupExists.IsFailure)
        {
            Result categoryExists = await CheckGroupCategoryExists(groupId.ToString()[..^2], cancellationToken);

            if (categoryExists.IsFailure)
            {
                Result categoryCreated = await CreateGroupCategory(groupId.ToString()[..^2], cancellationToken);

                if (categoryCreated.IsFailure)
                    return categoryCreated;
            }

            var groupCreated = await CreateGroup(groupId, cancellationToken);

            if (groupCreated.IsFailure)
                return groupCreated;
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

            return Result.Success();
        }

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Post, payload, cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CheckSectionExists(
        CanvasSectionCode sectionId,
        CancellationToken cancellationToken = default)
    {
        string path = $"sections/sis_section_id:{sectionId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CreateSection(
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

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CheckGroupExists(
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CheckGroupCategoryExists(
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        string path = $"group_categories/sis_group_category_id:{categoryId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CreateGroupCategory(
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

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    private async Task<Result> CreateGroup(
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

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }

    public async Task<Result<List<string>>> GetGroupMembers(
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        string path = $"groups/sis_group_id:{groupId}/memberships";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return returnData;

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        List<GroupMembershipListResult>? groupMembers = JsonConvert.DeserializeObject<List<GroupMembershipListResult>>(responseText);

        if (groupMembers is null)
            return Result.Failure<List<string>>(CanvasGatewayErrors.InvalidData);

        foreach (GroupMembershipListResult member in groupMembers)
        {
            Result<string> userId = await GetUserData(member.CanvasUserId, cancellationToken);

            if (userId.IsFailure)
                continue;

            returnData.Add(userId.Value);
        }

        return returnData;
    }

    private async Task<Result<string>> GetUserData(
        int canvasUserId,
        CancellationToken cancellationToken = default)
    {
        string path = $"users/{canvasUserId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Get, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure<string>(CanvasGatewayErrors.FailureResponseCode);

        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        UserDetailsEntry? userDetails = JsonConvert.DeserializeObject<UserDetailsEntry>(responseText);

        if (userDetails is null)
            return Result.Failure<string>(CanvasGatewayErrors.InvalidData);

        return userDetails.UserId;
    }

    public async Task<Result> RemoveUserFromGroup(
        string userId,
        CanvasSectionCode groupId,
        CancellationToken cancellationToken = default)
    {
        string path = $"groups/sis_group_id:{groupId}/users/sis_user_id:{userId}";

        HttpResponseMessage response = await RequestAsync(path, HttpVerb.Delete, cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(CanvasGatewayErrors.FailureResponseCode);
    }
}