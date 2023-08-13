namespace Constellation.Application.AdminDashboards.VerifySchoolContactAccess;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;

public sealed record VerifySchoolContactAccessQuery(
    int ContactId)
    : IQuery<UserAuditDto>;