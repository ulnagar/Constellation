namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal sealed class EnrolmentListEntry
{
    [JsonProperty("id")]
    public int EnrollmentId { get; set; }

    [JsonProperty("type")]
    public string EnrollmentType { get; set; }

    [JsonProperty("enrollment_state")]
    public string EnrollmentState { get; set; }

    [JsonProperty("sis_user_id")]
    public string SISId { get; set; }

    [JsonProperty("sis_section_id")]
    public string SectionId { get; set; }

    [JsonProperty("user")]
    public EnrolmentResultUser User { get; set; }

    internal class EnrolmentResultUser
    {
        [JsonProperty("login_id")]
        public string EmailAddress { get; set; }
    }
}