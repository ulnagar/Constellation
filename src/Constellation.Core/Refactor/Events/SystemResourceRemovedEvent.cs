namespace Constellation.Core.Refactor.Events;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Models;

public class SystemResourceRemovedEvent : BaseEvent
{
    public SystemResourceRemovedEvent(SystemResource resource)
    {
        Resource = resource;
    }

    public SystemResource Resource { get; }
}
