namespace Constellation.Application.Features.StaffMembers.Notifications;

using Constellation.Core.Models;
using MediatR;

public record StaffFacultyMembershipRemovedNotification : INotification
{
    public FacultyMembership Membership { get; init; }
}
