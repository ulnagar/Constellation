namespace Constellation.Application.Training.Modules.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetCountOfExpiringCertificatesForStaffMemberQuery(
    string StaffId)
    : IQuery<int>;
