namespace Constellation.Infrastructure.Features.Staff.Notifications.StaffFacultyMembershipAdded;

using Constellation.Application.Enums;
using Constellation.Application.Features.StaffMembers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public class AddToAdobeConnectFacultyGroup : INotificationHandler<StaffFacultyMembershipAddedNotification>
{
    private readonly IAppDbContext _context;

    public AddToAdobeConnectFacultyGroup(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StaffFacultyMembershipAddedNotification notification, CancellationToken cancellationToken)
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
            Action = AdobeConnectOperationAction.Add,
            DateScheduled = DateTime.Now
        };

        _context.AdobeConnectOperations.Add(operation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
