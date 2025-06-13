namespace Constellation.Core.Abstractions.Services;

using Models.StaffMembers.Identifiers;

public interface ICurrentUserService
{
    string UserName { get; }
    string EmailAddress { get; }
    bool IsAuthenticated { get; }

    StaffId StaffId { get; }
}
