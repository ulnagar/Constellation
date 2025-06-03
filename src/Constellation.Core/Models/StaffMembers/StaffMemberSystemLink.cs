namespace Constellation.Core.Models.StaffMembers;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Enums;
using Errors;
using Shared;
using StaffMembers.Identifiers;

public sealed class StaffMemberSystemLink : SystemLink
{
    private StaffMemberSystemLink(
        StaffId staffId,
        SystemType system,
        string value)
        : base(system, value)
    {
        StaffId = staffId;
    }

    public StaffId StaffId { get; private set; }

    internal static Result<StaffMemberSystemLink> Create(
        StaffId staffId,
        SystemType system,
        string value)
    {
        if (staffId == StaffId.Empty)
            return Result.Failure<StaffMemberSystemLink>(StaffMemberErrors.InvalidId);

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<StaffMemberSystemLink>(SystemLinkErrors.EmptyValue);

        return new StaffMemberSystemLink(
            staffId,
            system,
            value);
    }
}