namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Models;

public class FacultyHeadTeacherRemovedEvent : BaseEvent
{
    public FacultyHeadTeacherRemovedEvent(Faculty faculty, StaffMember staffMember)
    {
        StaffMember = staffMember;
        Faculty = faculty;
    }

    public Faculty Faculty { get; }
    public StaffMember StaffMember { get; }
}
