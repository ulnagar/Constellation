namespace Constellation.Core.Models.EmergencyConsole.Repositories;

using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IMessageTemplateRepository
{
    Task<List<MessageTemplate>> GetAll(CancellationToken cancellationToken = default);
    Task<MessageTemplate> GetById(TemplateId id, CancellationToken cancellationToken = default);
    Task<MessageTemplate?> GetByName(string name, CancellationToken cancellationToken = default);

    void Insert(MessageTemplate template);
    void Remove(MessageTemplate template);
}