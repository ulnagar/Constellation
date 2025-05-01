namespace Constellation.Application.Domains.StaffMembers.Queries.GetLifecycleDetailsForStaffMember;

using Abstractions.Messaging;
using Students.Queries.GetLifecycleDetailsForStudent;

public sealed record GetLifecycleDetailsForStaffMemberQuery(
    string StaffId)
: IQuery<RecordLifecycleDetailsResponse>;