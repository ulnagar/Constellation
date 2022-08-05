using MediatR;

namespace Constellation.Application.Features.Partners.SchoolContacts.Notifications
{
    public class SchoolContactRoleAssignmentCreatedNotification : INotification
    {
        public int AssignmentId { get; set; }
    }
}
