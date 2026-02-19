namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Models.AppSettings.Enums;

public sealed record SentralConfiguration
{
    public SentralConfiguration(
        SentralPath type,
        string path)
    {
        Type = type;
        Path = path;
    }

    public SentralPath Type { get; init; }
    public string Path { get; init; } = string.Empty;
}