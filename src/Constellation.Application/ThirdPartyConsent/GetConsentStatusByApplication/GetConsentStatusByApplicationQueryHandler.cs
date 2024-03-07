namespace Constellation.Application.ThirdPartyConsent.GetConsentStatusByApplication;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetConsentStatusByApplicationQueryHandler
:IQueryHandler<GetConsentStatusByApplicationQuery, List<ConsentStatusResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetConsentStatusByApplicationQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetConsentStatusByApplicationQuery>();
    }

    public async Task<Result<List<ConsentStatusResponse>>> Handle(GetConsentStatusByApplicationQuery request, CancellationToken cancellationToken)
    {
        List<ConsentStatusResponse> response = new();

        List<Student> students = new();

        if (request.OfferingCodes.Any() ||
            request.Grades.Any() ||
            request.SchoolCodes.Any())
            students.AddRange(await _studentRepository
                .GetFilteredStudents(
                    request.OfferingCodes,
                    request.Grades,
                    request.SchoolCodes,
                    cancellationToken));

        if (!students.Any())
        {
            _logger
                .ForContext(nameof(GetConsentStatusByApplicationQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NoneFoundFilter, true)
                .Warning("Failed to retrieve application while building list of Consent Statuses");

            return Result.Failure<List<ConsentStatusResponse>>(StudentErrors.NoneFoundFilter);
        }

        Application application = await _consentRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetConsentStatusByApplicationQuery), request, true)
                .ForContext(nameof(ApplicationId), request.ApplicationId, true)
                .ForContext(nameof(Error), ConsentErrors.Application.NotFound(request.ApplicationId), true)
                .Warning("Failed to retrieve application while building list of Consent Statuses");

            return Result.Failure<List<ConsentStatusResponse>>(ConsentErrors.Application.NotFound(request.ApplicationId));
        }

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (Student student in students)
        {
            string schoolName = string.Empty;

            School school = schools.FirstOrDefault(entry => entry.Code == student.SchoolCode);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(GetConsentStatusByApplicationQuery), request, true)
                    .ForContext("SchoolCode", student.SchoolCode)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(student.SchoolCode), true)
                    .Warning("Failed to retrieve application while building list of Consent Statuses");
            }
            else
            {
                schoolName = school.Name;
            }

            List<Transaction> transactions = await _consentRepository.GetTransactionsByStudentId(student.StudentId, cancellationToken);

            if (!transactions.Any())
            {
                response.Add(new(
                    student.StudentId,
                    student.GetName(),
                    student.CurrentGrade,
                    schoolName,
                    application.Name,
                    ConsentStatusResponse.ConsentStatus.Unknown,
                    DateOnly.MinValue));

                continue;
            }

            Consent consent = transactions
                .SelectMany(transaction =>
                    transaction.Consents
                        .Where(consent =>
                            consent.ApplicationId == request.ApplicationId))
                .MaxBy(consent => consent.ProvidedAt);

            if (consent is null)
            {
                response.Add(new(
                    student.StudentId,
                    student.GetName(),
                    student.CurrentGrade,
                    schoolName,
                    application.Name,
                    ConsentStatusResponse.ConsentStatus.Unknown,
                    DateOnly.MinValue));

                continue;
            }

            response.Add(new(
                student.StudentId,
                student.GetName(),
                student.CurrentGrade,
                schoolName,
                application.Name,
                consent.ConsentProvided ? ConsentStatusResponse.ConsentStatus.Granted : ConsentStatusResponse.ConsentStatus.Denied,
                DateOnly.FromDateTime(consent.ProvidedAt)));
        }

        return response;
    }
}
