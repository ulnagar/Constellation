namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal sealed class UserDetailsEntry
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("sis_user_id")]
    public string UserId { get; set; }

    [JsonProperty("email")]
    public string EmailAddress { get; set; }
}