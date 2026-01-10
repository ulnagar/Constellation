namespace Constellation.Application.Domains.Auth.Queries.GetAuthRolesAsSummary;

using Models.Identity.Enums;

public sealed record RoleSummaryResponse(
    Guid Id,
    string Name,
    AppRoleType Type,
    int Members);