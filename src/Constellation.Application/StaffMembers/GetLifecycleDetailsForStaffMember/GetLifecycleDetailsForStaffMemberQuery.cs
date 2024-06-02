namespace Constellation.Application.StaffMembers.GetLifecycleDetailsForStaffMember;

using Abstractions.Messaging;
using Students.GetLifecycleDetailsForStudent;

public sealed record GetLifecycleDetailsForStaffMemberQuery(
    string StaffId)
: IQuery<RecordLifecycleDetailsResponse>;