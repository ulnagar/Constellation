namespace Constellation.Application.Interfaces.Services;

using Core.Models.Messaging.Drafts;
using Core.Shared;

public interface IMessageDraftService
{
    Task<MessageDraft> GetDraft(Guid userId);
    Task<Result> AddRecipient(MessageRecipient recipient, Guid userId);
    Task<Result> RemoveRecipient(MessageRecipient recipient, Guid userId);
    Task<Result> UpdateDraft(MessageDraft draft);
    Task<Result> DeleteDraft(Guid userId);
}