namespace Constellation.Application.Domains.Training.Queries.GetCountOfExpiringCertificatesForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record GetCountOfExpiringCertificatesForStaffMemberQuery(
    StaffId StaffId)
    : IQuery<int>;
