namespace Constellation.Application.Training.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetCountOfExpiringCertificatesForStaffMemberQuery(
    string StaffId)
    : IQuery<int>;
