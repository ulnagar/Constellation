namespace Constellation.Core.Models.Messaging.EmergencyConsole.Repositories;

using Identifiers;

public interface IMessageEventRepository
{
    Task<List<MessageEvent>> GetAll(CancellationToken cancellationToken = default);
    Task<List<MessageEvent>> GetEventSummaries(CancellationToken cancellationToken = default);
    Task<MessageEvent?> GetEventById(EventId eventId, CancellationToken cancellationToken = default);
    Task<List<QueuedMessage>> GetQueuedMessagesByEventId(EventId eventId, CancellationToken cancellationToken = default);
    void Insert(MessageEvent message);
    void Insert(QueuedMessage item);
    void Remove(QueuedMessage item);
}