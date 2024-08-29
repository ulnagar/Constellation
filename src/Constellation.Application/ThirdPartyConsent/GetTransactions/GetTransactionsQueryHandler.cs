namespace Constellation.Application.ThirdPartyConsent.GetTransactions;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ThirdPartyConsent.Models;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class GetTransactionsQueryHandler
    : IQueryHandler<GetTransactionsQuery, List<TransactionSummaryResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTransactionsQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetTransactionsQuery>();
    }

    public async Task<Result<List<TransactionSummaryResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        List<TransactionSummaryResponse> response = new();

        List<Transaction> transactions = await _consentRepository.GetAllTransactions(cancellationToken);

        IEnumerable<IGrouping<string, Transaction>> transactionsByStudent = transactions.GroupBy(entry => entry.StudentId);

        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        foreach (IGrouping<string, Transaction> transactionList in transactionsByStudent)
        {

            Student student = await _studentRepository.GetWithSchoolBySRN(transactionList.Key, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(GetTransactionsQuery), request, true)
                    .ForContext(nameof(Student.StudentId), transactionList.Key)
                    .ForContext(nameof(Error), StudentErrors.NotFound(transactionList.Key), true)
                    .Warning("Failed to retrieve student while building list of Consent Transactions");

                continue;
            }

            if (student.IsDeleted)
                continue;

            foreach (Transaction transaction in transactionList)
            {
                List<TransactionSummaryResponse.ConsentStatusResponse> applicationEntries = new();

                foreach (Consent consent in transaction.Consents)
                {
                    Application application = applications.FirstOrDefault(entry => entry.Id == consent.ApplicationId);

                    if (application is null)
                    {
                        _logger
                            .ForContext(nameof(GetTransactionsQuery), request, true)
                            .ForContext(nameof(Consent), consent, true)
                            .ForContext(nameof(ApplicationId), consent.ApplicationId, true)
                            .ForContext(nameof(Error), ConsentErrors.Application.NotFound(consent.ApplicationId), true)
                            .Warning("Failed to retrieve application while building list of Consent Transactions");

                        continue;
                    }

                    bool? mostRecent = await _consentRepository.IsMostRecentResponse(consent.Id, cancellationToken);

                    if (mostRecent is null)
                    {
                        _logger
                            .ForContext(nameof(GetTransactionsQuery), request, true)
                            .ForContext(nameof(Consent), consent, true)
                            .ForContext(nameof(ConsentId), consent.Id, true)
                            .ForContext(nameof(Error), ConsentErrors.Consent.NotFound(consent.Id), true)
                            .Warning("Failed to retrieve consent response while building list of Consent Transactions");

                        continue;
                    }

                    applicationEntries.Add(new(
                        application.Name,
                        consent.ConsentProvided,
                        mostRecent.Value));
                }

                response.Add(new(
                    transaction.Id,
                    student.StudentId,
                    student.GetName(),
                    student.CurrentGrade,
                    student.School.Name,
                    transaction.SubmittedBy,
                    transaction.SubmittedAt,
                    transaction.SubmissionMethod,
                    transaction.SubmissionNotes,
                    applicationEntries));
            }
        }

        return response;
    }
}
