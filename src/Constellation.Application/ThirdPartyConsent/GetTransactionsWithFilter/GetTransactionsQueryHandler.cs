namespace Constellation.Application.ThirdPartyConsent.GetTransactionsWithFilter;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ThirdPartyConsent.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Constellation.Core.Shared;
using GetTransactions;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class GetTransactionsWithFilterQueryHandler
    : IQueryHandler<GetTransactionsWithFilterQuery, List<TransactionSummaryResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTransactionsWithFilterQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetTransactionsQuery>();
    }

    public async Task<Result<List<TransactionSummaryResponse>>> Handle(GetTransactionsWithFilterQuery request, CancellationToken cancellationToken)
    {
        List<TransactionSummaryResponse> response = new();

        List<Student> students = new();

        if (!request.StudentIds.Any() &&
            !request.OfferingCodes.Any() &&
            !request.Grades.Any() &&
            !request.SchoolCodes.Any())
        {
            // students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);
            return response;
        }

        if (request.StudentIds.Any())
            students.AddRange(await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken));

        if (request.OfferingCodes.Any() ||
            request.Grades.Any() ||
            request.SchoolCodes.Any())
            students.AddRange(await _studentRepository
                .GetFilteredStudents(
                    request.OfferingCodes,
                    request.Grades,
                    request.SchoolCodes,
                    cancellationToken));

        students = students
            .Distinct()
            .ToList();

        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        foreach (Student student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName))
        {
            List<Transaction> transactions =
                await _consentRepository.GetTransactionsByStudentId(student.StudentId, cancellationToken);

            foreach (Transaction transaction in transactions)
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

                    if (mostRecent is false)
                        continue;

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
