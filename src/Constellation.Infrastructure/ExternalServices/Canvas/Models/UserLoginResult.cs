namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class UserLoginResult
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
