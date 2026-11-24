namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Models.EnrolmentContext.EnrolmentUser.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

internal sealed class EnrolmentUserRepository
    : IEnrolmentUserRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentUserRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<EnrolmentUser?> GetById(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentUser>()
            .FirstOrDefaultAsync(
                user => user.Id == userId,
                cancellationToken);

    public async Task<EnrolmentUser?> GetByEmail(
        EmailAddress email, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentUser>()
            .FirstOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);

    public async Task<List<EnrolmentUser>> GetListFromIds(
        List<Guid> userIds, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentUser>()
            .Where(user => userIds.Contains(user.Id))
            .ToListAsync(cancellationToken);

    public async Task<EnrolmentUserPasskey?> GetPasskeyById(
        byte[] id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentUserPasskey>()
            .FirstOrDefaultAsync(entry => entry.CredentialId == id, cancellationToken);

    public async Task<bool> DoesCredentialAlreadyExist(
        byte[] id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentUserPasskey>()
            .AnyAsync(entry => entry.CredentialId == id, cancellationToken);
}
