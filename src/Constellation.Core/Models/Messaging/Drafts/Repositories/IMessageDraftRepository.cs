namespace Constellation.Core.Models.Messaging.Drafts.Repositories;

using Constellation.Core.Models.Messaging.Drafts.Enums;
using Identifiers;
using Shared;
using System;

public interface IMessageDraftRepository
{
    Task<MessageDraft> GetDraft(Guid userId, string module = "", CancellationToken cancellationToken = default);
    Task<Result> AddRecipient(MessageRecipient recipient, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveRecipient(MessageRecipientId recipientId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateDraft(Guid userId, Action<MessageDraft> apply, CancellationToken cancellationToken = default);
    Task<Result> SendDraft(Guid userId, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task DeleteDraft(Guid userId, CancellationToken cancellationToken = default);
}
