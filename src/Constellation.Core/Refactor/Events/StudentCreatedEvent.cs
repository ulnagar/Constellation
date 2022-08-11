namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Models;

public class StudentCreatedEvent : BaseEvent
{
    public StudentCreatedEvent(Student student)
    {
        Student = student;
    }

    public Student Student { get; }
}
