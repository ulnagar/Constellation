namespace Constellation.Application.Domains.StaffMembers.Queries.GetLifecycleDetailsForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Students.Queries.GetLifecycleDetailsForStudent;

public sealed record GetLifecycleDetailsForStaffMemberQuery(
    StaffId StaffId)
: IQuery<RecordLifecycleDetailsResponse>;