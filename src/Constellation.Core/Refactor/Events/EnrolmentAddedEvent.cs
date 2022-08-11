namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Models;
using Constellation.Core.Refactor.Common;

public class EnrolmentAddedEvent : BaseEvent
{
    public EnrolmentAddedEvent(Enrolment enrolment)
    {
        Enrolment = enrolment;
    }

    public Enrolment Enrolment { get; }
}
