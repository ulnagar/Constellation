namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentStatusByApplication;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
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

        if (request.OfferingCodes.Count > 0 ||
            request.Grades.Count > 0)
            students = await _studentRepository.GetFilteredStudents(
                request.OfferingCodes,
                request.Grades,
                new(),
                cancellationToken);

        if (students.Count == 0)
        {
            _logger
                .ForContext(nameof(GetConsentStatusByApplicationQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NoneFoundFilter, true)
                .Warning("Failed to retrieve application while building list of Consent Statuses");

            return Result.Failure<List<ConsentStatusResponse>>(StudentErrors.NoneFoundFilter);
        }

        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        foreach (Application application in applications)
        {
            List<Name> consentGranted = new();
            List<Name> consentDenied = new();
            List<Name> consentPending = new();

            if (application.ConsentRequired)
            {
                foreach (Student student in students)
                {
                    Consent consent = application.Consents
                        .Where(consent => consent.StudentId == student.Id)
                        .MaxBy(consent => consent.ProvidedAt);

                    switch (consent?.ConsentProvided)
                    {
                        case true:
                            consentGranted.Add(student.Name);
                            break;
                        case false:
                            consentDenied.Add(student.Name);
                            break;
                        default:
                            consentPending.Add(student.Name);
                            break;
                    }
                }
            }
            
            response.Add(new(
                application.Id,
                application.Name,
                application.ConsentRequired,
                consentGranted,
                consentDenied,
                consentPending));
        }

        return response;
    }
}
