namespace Constellation.Core.Models.EmergencyConsole.Repositories;

using Identifiers;

public interface ISentMessageRepository
{
    Task<List<SentMessage>> GetAll(CancellationToken cancellationToken = default);
    Task<List<SentMessage>> GetMessageSummaries(CancellationToken cancellationToken = default);
    Task<SentMessage?> GetMessageById(EventId eventId, CancellationToken cancellationToken = default);
    void Insert(SentMessage message);
}