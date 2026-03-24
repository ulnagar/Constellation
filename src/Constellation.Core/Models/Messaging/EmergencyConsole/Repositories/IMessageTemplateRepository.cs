namespace Constellation.Core.Models.Messaging.EmergencyConsole.Repositories;

using Identifiers;

public interface IMessageTemplateRepository
{
    Task<List<MessageTemplate>> GetAll(CancellationToken cancellationToken = default);
    Task<MessageTemplate> GetById(TemplateId id, CancellationToken cancellationToken = default);
    Task<MessageTemplate?> GetByName(string name, CancellationToken cancellationToken = default);

    void Insert(MessageTemplate template);
    void Remove(MessageTemplate template);
}