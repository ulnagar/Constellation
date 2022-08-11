namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Models;
using Constellation.Core.Refactor.Common;

public class EnrolmentRemovedEvent : BaseEvent
{
    public EnrolmentRemovedEvent(Enrolment enrolment)
    {
        Enrolment = enrolment;
    }

    public Enrolment Enrolment { get; }
}
