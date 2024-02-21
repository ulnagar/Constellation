namespace Constellation.Application.AdminDashboards.VerifySchoolContactAccess;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.SchoolContacts.Identifiers;

public sealed record VerifySchoolContactAccessQuery(
    SchoolContactId ContactId)
    : IQuery<UserAuditDto>;