namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Models;

public class SystemResourceAddedEvent : BaseEvent
{
    public SystemResourceAddedEvent(SystemResource resource)
    {
        Resource = resource;
    }

    public SystemResource Resource { get; }
}
