using MediatR;

namespace Constellation.Application.Features.Partners.SchoolContacts.Notifications
{
    public class SchoolContactCreatedNotification : INotification
    {
        public int Id { get; set; }
        public bool SelfRegistered { get; set; }
    }
}
