namespace Constellation.Core.Models.ThirdPartyConsent.Repositories;

using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Identifiers.ApplicationId;

public interface IConsentRepository
{
    Task<Application> GetApplicationById(ApplicationId applicationId, CancellationToken cancellationToken = default);
    Task<List<Application>> GetAllActiveApplications(CancellationToken cancellationToken = default);

    Task<Transaction> GetTransactionById(ConsentTransactionId transactionId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetTransactionsByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetAllTransactions(CancellationToken cancellationToken = default);

    void Insert(Application application);
    void Insert(Transaction transaction);
}
