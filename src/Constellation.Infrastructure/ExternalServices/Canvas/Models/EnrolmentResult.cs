namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class EnrolmentResult
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
