using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    internal class CanvasGateway : ICanvasGateway, IScopedService
    {
        private readonly HttpClient _client;

        private readonly string _url;
        private readonly string _apiKey;

        public CanvasGateway(ICanvasGatewayConfiguration configuration)
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

            HttpResponseMessage response;

            switch (action)
            {
                case HttpVerb.Get:
                    response = await _client.GetAsync(uri.ToString());

                    break;
                case HttpVerb.Post:
                    response = await _client.PostAsJsonAsync(uri.ToString(), payload);

                    break;
                case HttpVerb.Put:
                    response = await _client.PutAsJsonAsync(uri.ToString(), payload);

                    break;
                case HttpVerb.Delete:
                    response = await _client.DeleteAsync(uri.ToString());

                    break;
                default:
                    response = new HttpResponseMessage();
                    response.StatusCode = HttpStatusCode.BadRequest;

                    break;
            }

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
    }
}
