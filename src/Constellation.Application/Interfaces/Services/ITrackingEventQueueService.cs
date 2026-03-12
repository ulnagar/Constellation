namespace Constellation.Application.Interfaces.Services;

using Core.Models.Messaging.Tracking;

public interface ITrackingEventQueueService
{
    Task EnqueueAsync(TrackingEvent evt);
}
