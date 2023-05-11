namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class FileUploadLocationResult
{
    [JsonProperty("upload_url")]
    public string UploadUrl { get; set; }
}
