namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json.Serialization;

public interface ILissResponse { }

public sealed class LissResponse : ILissResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("result")]
    public LissResponseBase Result { get; set; }
}

public abstract class LissResponseBase { }

public sealed class LissResponseHello : LissResponseBase
{
    [JsonPropertyName("SIS")]
    public string System => "Constellation";

    [JsonPropertyName("LissVersion")]
    public int LissVersion => 10000;
}

public sealed class LissResponseError : ILissResponse
{
    public static readonly LissResponseError NotValid = new("Invalid Call");
    public static readonly LissResponseError InvalidAuthentication = new("Invalid Authentication Object");
    public static readonly LissResponseError InvalidParameters = new("Invalid parameters provided!");

    private LissResponseError(string error)
        => Error = error;
    
    [JsonPropertyName("error")]
    public string Error { get; init; }
}

public sealed class LissResponseBlank : ILissResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("result")]
    public string Result => "";
}