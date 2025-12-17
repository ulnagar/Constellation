namespace Constellation.Core.Models.EmergencyConsole.Repositories;

using Identifiers;

public interface ISentMessageRepository
{
    Task<List<SentMessage>> GetAll(CancellationToken cancellationToken = default);
    Task<SentMessage?> GetForMessage(EventId eventId, CancellationToken cancellationToken = default);
    void Insert(SentMessage message);
}