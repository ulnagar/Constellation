namespace Constellation.Infrastructure.Features.Staff.Notifications.StaffFacultyMembershipRemoved;

using Constellation.Application.Enums;
using Constellation.Application.Features.StaffMembers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RemoveFromAdobeConnectFacultyGroup : INotificationHandler<StaffFacultyMembershipRemovedNotification>
{
    private readonly IAppDbContext _context;

    public RemoveFromAdobeConnectFacultyGroup(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StaffFacultyMembershipRemovedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.Membership.Faculty.Name.Contains("Administration"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Administration, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("Executive"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Executive, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("English"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.English, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("Mathematics"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Mathematics, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("Science"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Science, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("Stage3"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Stage3, cancellationToken);
        }

        if (notification.Membership.Faculty.Name.Contains("Support"))
        {
            await CreateAdobeConnectGroupMembershipOperation(notification.Membership.StaffId, AdobeConnectGroup.Support, cancellationToken);
        }
    }

    private async Task CreateAdobeConnectGroupMembershipOperation(string staffId, AdobeConnectGroup group, CancellationToken cancellationToken)
    {
        var operation = new TeacherAdobeConnectGroupOperation
        {
            GroupSco = ((int)group).ToString(),
            GroupName = group.ToString(),
            TeacherId = staffId,
            Action = AdobeConnectOperationAction.Remove,
            DateScheduled = DateTime.Now
        };

        _context.AdobeConnectOperations.Add(operation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
