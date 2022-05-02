using MediatR;

namespace Constellation.Application.Features.Partners.Students.Notifications
{
    public class StudentWithdrawnNotification : INotification
    {
        public string StudentId { get; set; }
    }
}
