using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.ExternalServices.Canvas
{
    internal class Gateway : ICanvasGateway, IScopedService
    {
        private readonly HttpClient _client;

        private readonly string _url;
        private readonly string _apiKey;

        public Gateway(ICanvasGatewayConfiguration configuration)
        {
            _url = configuration.ApiEndpoint;
            _apiKey = configuration.ApiKey;

            var config = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            var proxy = WebRequest.DefaultWebProxy;
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

        private async Task<HttpResponseMessage> RequestAsync(string path, HttpVerb action, object payload = null)
        {
            var uri = new Uri($"{_url}/{path}");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            HttpResponseMessage response = action switch
            {
                HttpVerb.Get => await _client.GetAsync(uri.ToString()),
                HttpVerb.Post => await _client.PostAsJsonAsync(uri.ToString(), payload),
                HttpVerb.Put => await _client.PutAsJsonAsync(uri.ToString(), payload),
                HttpVerb.Delete => await _client.DeleteAsync(uri.ToString()),
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

            var response = await RequestAsync(path, HttpVerb.Get);
            var responseText = await response.Content.ReadAsStringAsync();

            var users = JsonConvert.DeserializeObject<List<UserResult>>(responseText);

            return users.FirstOrDefault(login => login.SISId == UserId)?.Id;
        }

        private async Task<int?> SearchForCourseEnrolment(string UserId, string CourseId)
        {
            var path = $"courses/sis_course_id:{CourseId}/enrollments?sis_user_id={UserId}";

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

            var response = await RequestAsync(path, HttpVerb.Get);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseText = await response.Content.ReadAsStringAsync();

            var assignments = JsonConvert.DeserializeObject<List<AssignmentResult>>(responseText);

            return assignments;
        }

        public async Task<bool> UploadAssignmentSubmission(string CourseId, int CanvasAssignmentId, string StudentId, StoredFile file)
        {
            var stepOnePath = $"courses/sis_course_id:{CourseId}/assignments/{CanvasAssignmentId}/submissions/sis_user_id:{StudentId}/files";

            var stepOnePayload = new
            {
                name = file.Name,
                size = file.FileData.Length,
                content_type = file.FileType,
                on_duplicate = "overwrite"
            };

            var stepOneResponse = await RequestAsync(stepOnePath, HttpVerb.Post, stepOnePayload);
            if (!stepOneResponse.IsSuccessStatusCode)
                return false;

            var stepOneResponseText = await stepOneResponse.Content.ReadAsStringAsync();
            var fileUploadLocation = JsonConvert.DeserializeObject<FileUploadLocationResult>(stepOneResponseText);

            int fileId = 0;

            // MultipartFormDataContent code taken from https://makolyte.com/csharp-how-to-send-a-file-with-httpclient/
            using (var stepTwoContent = new MultipartFormDataContent())
            {
                stepTwoContent.Add(new StringContent(file.Name), name: "filename");
                stepTwoContent.Add(new StringContent(file.FileType), name: "content_type");

                var fileStream = new MemoryStream(file.FileData);
                var fileStreamContent = new StreamContent(fileStream);
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.FileType);
                stepTwoContent.Add(fileStreamContent, name: "file", fileName: file.Name);

                var stepTwoResponse = await _client.PostAsync(fileUploadLocation.UploadUrl, stepTwoContent);
                if (!stepTwoResponse.IsSuccessStatusCode)
                    return false;

                var stepTwoResponseText = await stepTwoResponse.Content.ReadAsStringAsync();
                var fileUploadConfirmation = JsonConvert.DeserializeObject<FileUploadConfirmationResult>(stepTwoResponseText);
                fileId = fileUploadConfirmation.Id;
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
                return false;

            return true;
        }

        public async Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(string CourseId)
        {
            var path = $"courses/sis_course_id:{CourseId}/assignments";

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

            var response = await RequestAsync(path, HttpVerb.Put, payload);

            if (response.IsSuccessStatusCode)
            {
                path = $"accounts/1/users/{CanvasUserId}";

                response = await RequestAsync(path, HttpVerb.Delete);

                if (response.IsSuccessStatusCode)
                    return true;
            }

            return false;
        }

        private class UserResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("sortable_name")]
            public string SortableName { get; set; }

            [JsonProperty("short_name")]
            public string ShortName { get; set; }

            [JsonProperty("sis_user_id")]
            public string SISId { get; set; }

            [JsonProperty("login_id")]
            public string LoginUsername { get; set; }
        }

        private class UserLoginResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("user_id")]
            public int UserId { get; set; }

            [JsonProperty("workflow_state")]
            public string State { get; set; }

            [JsonProperty("unique_id")]
            public string Username { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("sis_user_id")]
            public string SISId { get; set; }
        }

        private class EnrolmentResult
        {
            [JsonProperty("course_id")]
            public int CourseId { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("user_id")]
            public int UserId { get; set; }

            [JsonProperty("type")]
            public string EnrollmentType { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("enrollment_state")]
            public string EnrollmentState { get; set; }

            [JsonProperty("sis_course_id")]
            public string SISCourseId { get; set; }

            [JsonProperty("sis_user_id")]
            public string SISId { get; set; }
        }

        private class AssignmentResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            /// <summary>
            /// The date the assignment is due.
            /// </summary>
            [JsonProperty("due_at")]
            public DateTime? DueDate { get; set; }
            /// <summary>
            /// The date after which submissions are accepted.
            /// </summary>
            [JsonProperty("unlock_at")]
            public DateTime? UnlockDate { get; set; }
            /// <summary>
            /// The date after which no more submission are accepted.
            /// </summary>
            [JsonProperty("lock_at")]
            public DateTime? LockDate { get; set; }
            [JsonProperty("allowed_attempts")]
            public int AllowedAttempts { get; set; }
            [JsonProperty("submission_types")]
            public ICollection<string> SubmissionTypes { get; set; }
            [JsonProperty("published")]
            public bool IsPublished { get; set; }
        }

        private class FileUploadLocationResult
        {
            [JsonProperty("upload_url")]
            public string UploadUrl { get; set; }
        }

        private class FileUploadConfirmationResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }
        }
    }
}
