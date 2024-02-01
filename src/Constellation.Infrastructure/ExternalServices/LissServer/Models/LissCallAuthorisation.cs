namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json.Serialization;

public class LissCallAuthorisation
{
    [JsonPropertyName("School")]
    public string School { get; set; }

    [JsonPropertyName("UserName")]
    public string UserName { get; set; }

    [JsonPropertyName("Password")]
    public string Password { get; set; }

    [JsonPropertyName("LissVersion")]
    public int LissVersion { get; set; }

    [JsonPropertyName("UserAgent")]
    public string UserAgent { get; set; }
}