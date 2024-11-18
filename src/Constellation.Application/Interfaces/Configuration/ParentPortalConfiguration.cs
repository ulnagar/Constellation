namespace Constellation.Application.Interfaces.Configuration;

public sealed class ParentPortalConfiguration
{
    public const string Section = "Constellation:ParentPortal";

    public bool ShowConsent { get; set; }
}