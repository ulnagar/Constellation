namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Email.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

internal sealed class EmailRepository : IEmailRepository
{
    private readonly AppDbContext _context;

    public EmailRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmailMessage?> GetById(
        EmailId id, 
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<EmailMessage>()
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken);

    public async Task<List<EmailMessage>> GetByRecipient(
        EmailAddress email, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EmailMessage>()
            .Where(message => 
                message.Recipients.Any(recipient => 
                    recipient.Email == email))
            .ToListAsync(cancellationToken);

    public async Task<List<EmailMessage>> GetRecent(
        int count,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EmailMessage>()
            .OrderByDescending(message => message.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public void Insert(EmailMessage message) => 
        _context.Set<EmailMessage>().Add(message);
}