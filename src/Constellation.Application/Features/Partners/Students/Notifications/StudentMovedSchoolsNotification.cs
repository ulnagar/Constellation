using MediatR;

namespace Constellation.Application.Features.Partners.Students.Notifications
{
    public class StudentMovedSchoolsNotification : INotification
    {
        public string StudentId { get; set; }
        public string OldSchoolCode { get; set; }
        public string NewSchoolCode { get; set; }
    }
}
