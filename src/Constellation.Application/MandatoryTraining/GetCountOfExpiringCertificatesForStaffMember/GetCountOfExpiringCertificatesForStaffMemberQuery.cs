namespace Constellation.Application.MandatoryTraining.GetCountOfExpiringCertificatesForStaffMember;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetCountOfExpiringCertificatesForStaffMemberQuery(
    string StaffId) 
    : IQuery<int>;
