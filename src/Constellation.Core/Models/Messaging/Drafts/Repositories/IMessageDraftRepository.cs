namespace Constellation.Core.Models.Messaging.Drafts.Repositories;

using Identifiers;
using Shared;
using System;

public interface IMessageDraftRepository
{
    Task<MessageDraft> GetDraft(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> AddRecipient(MessageRecipient recipient, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveRecipient(MessageRecipientId recipientId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateDraft(MessageDraft draft, CancellationToken cancellationToken = default);
    Task DeleteDraft(Guid userId, CancellationToken cancellationToken = default);
}
