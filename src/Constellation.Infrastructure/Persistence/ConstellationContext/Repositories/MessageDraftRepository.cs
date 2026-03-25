namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Messaging.Drafts.Errors;
using Core.Models.Messaging.Drafts;
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

    public async Task<MessageDraft> GetDraft(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        bool exists = await context
            .Set<MessageDraft>()
            .AnyAsync(draft => draft.UserId == userId, cancellationToken);

        if (!exists)
        {
            MessageDraft newDraft = new(userId);
            context.Set<MessageDraft>().Add(newDraft);
            await context.SaveChangesAsync(cancellationToken);
        }

        return await context
            .Set<MessageDraft>()
            .AsNoTracking()
            .SingleAsync(draft => draft.UserId == userId,
                cancellationToken);
    }

    public async Task<Result> AddRecipient(
        MessageRecipient recipient,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft? draft = await context
            .Set<MessageDraft>()
            .SingleOrDefaultAsync(draft => draft.UserId == userId,
                cancellationToken);

        if (draft is null)
        {
            draft = new(userId);
            context.Set<MessageDraft>().Add(draft);
            await context.SaveChangesAsync(cancellationToken);
        }

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
        MessageRecipient recipient,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        MessageDraft? draft = await context
            .Set<MessageDraft>()
            .SingleOrDefaultAsync(draft => draft.UserId == userId,
                cancellationToken);

        if (draft is null)
            return Result.Failure(MessageDraftErrors.NotFound);

        if (!draft.Recipients.Contains(recipient))
            return Result.Failure(MessageDraftErrors.RemoveRecipient.NotFound);

        draft.RemoveRecipient(recipient);
        draft.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateDraft(
        MessageDraft draft,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        context.Set<MessageDraft>().Attach(draft);
        draft.UpdatedAt = DateTimeOffset.UtcNow;
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
