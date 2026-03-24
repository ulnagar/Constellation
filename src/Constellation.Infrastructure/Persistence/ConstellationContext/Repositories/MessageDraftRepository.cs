namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

internal sealed class MessageDraftRepository : IMessageDraftRepository
{
    private readonly AppDbContext _context;

    public MessageDraftRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<MessageDraft?> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageDraft>()
            .SingleOrDefaultAsync(draft => draft.UserId == userId, 
                cancellationToken);

    public async Task Clear(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageDraft>()
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

    public void Insert(MessageDraft draft) => _context.Set<MessageDraft>().Add(draft);
}
