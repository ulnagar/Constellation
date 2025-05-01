namespace Constellation.Application.Domains.ExternalSystems.Masterfile.Models;

public sealed record MasterFileSchool(
    int Index,
    string SiteCode,
    string Name,
    SiteStatus Status,
    string PrincipalName,
    string PrincipalEmail);

public enum SiteStatus
{
    Inactive,
    Students,
    Teachers,
    Both,
    Unknown
}