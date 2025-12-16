namespace Constellation.Core.Models.EmergencyConsole.Repositories;

using Identifiers;

public interface ISentMessageRepository
{
    Task<List<SentMessage>> GetAll(CancellationToken cancellationToken = default);
    Task<List<SentMessage>> GetForMessage(MessageId  messageId, CancellationToken cancellationToken = default);
    void Insert(SentMessage message);
}