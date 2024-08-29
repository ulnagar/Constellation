namespace Constellation.Application.ThirdPartyConsent.GetTransactionDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTransactionDetailsQueryHandler
: IQueryHandler<GetTransactionDetailsQuery, TransactionDetailsResponse>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTransactionDetailsQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetTransactionDetailsQuery>();
    }

    public async Task<Result<TransactionDetailsResponse>> Handle(GetTransactionDetailsQuery request, CancellationToken cancellationToken)
    {
        Transaction transaction = await _consentRepository.GetTransactionById(request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            _logger
                .ForContext(nameof(GetTransactionDetailsQuery), request, true)
                .ForContext(nameof(Error), ConsentErrors.Transaction.NotFound(request.TransactionId), true)
                .Warning("Failed to retrieve Consent Transaction details");

            return Result.Failure<TransactionDetailsResponse>(ConsentErrors.Transaction.NotFound(request.TransactionId));
        }

        Student student = await _studentRepository.GetWithSchoolBySRN(transaction.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetTransactionDetailsQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(transaction.StudentId), true)
                .Warning("Failed to retrieve Consent Transaction details");

            return Result.Failure<TransactionDetailsResponse>(StudentErrors.NotFound(transaction.StudentId));
        }

        List<TransactionDetailsResponse.ConsentResponse> consentResponses = new();

        foreach (Consent consent in transaction.Consents)
        {
            bool? mostRecent = await _consentRepository.IsMostRecentResponse(consent.Id, cancellationToken);

            if (mostRecent is null)
            {
                _logger
                    .ForContext(nameof(GetTransactionDetailsQuery), request, true)
                    .ForContext(nameof(Consent), consent, true)
                    .ForContext(nameof(Error), ConsentErrors.Consent.NotFound(consent.Id), true)
                    .Warning("Failed to retrieve Consent Transaction details");

                continue;
            }

            Application application = await _consentRepository.GetApplicationById(consent.ApplicationId, cancellationToken);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(GetTransactionDetailsQuery), request, true)
                    .ForContext(nameof(Consent), consent, true)
                    .ForContext(nameof(Error), ConsentErrors.Application.NotFound(consent.ApplicationId), true)
                    .Warning("Failed to retrieve Consent Transaction details");

                continue;
            }

            consentResponses.Add(new(
                consent.Id,
                consent.ApplicationId,
                application.Name,
                consent.ConsentProvided,
                mostRecent.Value));
        }

        return new TransactionDetailsResponse(
            transaction.Id,
            transaction.StudentId,
            student.GetName(),
            student.CurrentGrade,
            student.School.Name,
            transaction.SubmittedBy,
            transaction.SubmittedAt,
            transaction.SubmissionMethod,
            transaction.SubmissionNotes,
            consentResponses);
    }
}
