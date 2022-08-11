namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Models;

public class StudentDeletedEvent : BaseEvent
{
    public StudentDeletedEvent(Student student)
    {
        Student = student;
    }

    public Student Student { get; }
}
