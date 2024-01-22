namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class ConsentRepository : IConsentRepository
{
    private readonly AppDbContext _context;

    public ConsentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Application> GetApplicationById(
        ApplicationId applicationId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application => application.Id == applicationId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Application>> GetAllActiveApplications(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<Application>()
            .Where(application => !application.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetAllApplications(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .ToListAsync(cancellationToken);

    public async Task<Transaction> GetTransactionById(
        ConsentTransactionId transactionId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Transaction>()
            .Where(transaction => transaction.Id == transactionId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Transaction>> GetTransactionsByStudentId(
        string studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Transaction>()
            .Where(transaction => transaction.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<List<Transaction>> GetAllTransactions(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Transaction>()
            .ToListAsync(cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);

    public void Insert(Transaction transaction) => _context.Set<Transaction>().Add(transaction);
}
