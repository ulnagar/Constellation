namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class FileUploadConfirmationResult
{
    [JsonProperty("id")]
    public int Id { get; set; }
}
