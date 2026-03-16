namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Email.Repositories;
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

    public void Insert(EmailMessage message) => 
        _context.Set<EmailMessage>().Add(message);
}