namespace Constellation.Core.Models.Messaging.Drafts.Repositories;

using System;

public interface IMessageDraftRepository
{
    Task<MessageDraft?> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task Clear(Guid userId, CancellationToken cancellationToken = default);
    void Insert(MessageDraft draft);
}
