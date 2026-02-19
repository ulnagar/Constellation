namespace Constellation.Core.Models.AppSettings;

using Enums;

public sealed class SentralSettings
{
    private SentralSettings() { }

    public SentralSettings(
        SentralPath type,
        string path)
    {
        Type = type;
        Path = path;
    }

    public SentralPath Type { get; private set; }
    public string Path { get; private set; } = string.Empty;
}