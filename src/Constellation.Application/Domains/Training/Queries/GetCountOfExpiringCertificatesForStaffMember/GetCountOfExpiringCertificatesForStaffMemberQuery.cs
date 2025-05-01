namespace Constellation.Application.Domains.Training.Queries.GetCountOfExpiringCertificatesForStaffMember;

using Abstractions.Messaging;

public sealed record GetCountOfExpiringCertificatesForStaffMemberQuery(
    string StaffId)
    : IQuery<int>;
