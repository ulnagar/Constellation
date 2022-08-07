using MediatR;
using System;

namespace Constellation.Application.Features.Lessons.Notifications
{
    public class StudentMarkedPresentInScienceLessonNotification : INotification
    {
        public Guid RollId { get; set; }
        public string StudentId { get; set; }
    }
}
