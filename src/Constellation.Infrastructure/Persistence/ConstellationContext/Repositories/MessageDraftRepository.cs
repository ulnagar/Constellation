namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Messaging.Drafts.Errors;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Identifiers;
using Core.Models.Messaging.Drafts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;

internal sealed class MessageDraftRepository : IMessageDraftRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public MessageDraftRepository(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private async Task<MessageDraft> GetOrCreateDraft(
        AppDbContext context,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        MessageDraft? draft = await context
            .Set<MessageDraft>()
            .FirstOrDefaultAsync(draft => draft.UserId == userId, cancellationToken);

        if (draft is not null)
            return draft;

        draft = new(userId);
        context.Set<MessageDraft>().Add(draft);
        await context.SaveChangesAsync(cancellationToken);

        return draft;
    }

    public async Task<MessageDraft> GetDraft(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await GetOrCreateDraft(context, userId, cancellationToken);
    }

    public async Task<Result> AddRecipient(
        MessageRecipient recipient,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft draft = await GetOrCreateDraft(context, userId, cancellationToken);

        if (recipient.EmailAddress != EmailAddress.None && draft.Recipients.Any(entry => entry.EmailAddress == recipient.EmailAddress))
            return Result.Failure(MessageDraftErrors.AddRecipient.DuplicateEmailFound);

        if (recipient.PhoneNumber != PhoneNumber.Empty && draft.Recipients.Any(entry => entry.PhoneNumber == recipient.PhoneNumber))
            return Result.Failure(MessageDraftErrors.AddRecipient.DuplicatePhoneNumberFound);

        draft.AddRecipient(recipient);
        draft.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveRecipient(
        MessageRecipientId recipientId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft draft = await GetOrCreateDraft(context, userId, cancellationToken);

        MessageRecipient? recipient = draft.Recipients.FirstOrDefault(recipient => recipient.Id == recipientId);

        if (recipient is null)
            return Result.Failure(MessageDraftErrors.RemoveRecipient.NotFound);

        draft.RemoveRecipient(recipient);
        draft.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateDraft(
        Guid userId,
        Action<MessageDraft> apply,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft draft = await GetOrCreateDraft(context, userId, cancellationToken);

        apply(draft);
        draft.UpdatedAt = DateTimeOffset.UtcNow;
        
        await context.SaveChangesAsync(cancellationToken); 
        return Result.Success();
    }

    public async Task<Result> SendDraft(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft? draft = await context
            .Set<MessageDraft>()
            .FirstOrDefaultAsync(draft => draft.UserId == userId, cancellationToken);

        if (draft is null)
            return Result.Failure(MessageDraftErrors.NotFound);

        QueuedMessage queued = QueuedMessage.FromDraft(draft);

        context.Set<QueuedMessage>().Add(queued);
        context.Set<MessageDraft>().Remove(draft);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }

    public async Task DeleteDraft(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await context
            .Set<MessageDraft>()
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
