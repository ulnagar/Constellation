namespace Constellation.Application.ThirdPartyConsent.GetConsentStatusByApplication;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
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
                .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(request.ApplicationId), true)
                .Warning("Failed to retrieve application while building list of Consent Statuses");

            return Result.Failure<List<ConsentStatusResponse>>(ConsentApplicationErrors.NotFound(request.ApplicationId));
        }

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;
            
            if (enrolment is null)
            {
                _logger
                    .ForContext(nameof(GetConsentStatusByApplicationQuery), request, true)
                    .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                    .Warning("Failed to retrieve application while building list of Consent Statuses");

                continue;
            }

            Consent consent = application.Consents
                .Where(consent => consent.StudentId == student.Id)
                .MaxBy(consent => consent.ProvidedAt);

            if (consent is null)
            {
                response.Add(new(
                    student.Id,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    application.Name,
                    ConsentStatusResponse.ConsentStatus.Unknown,
                    DateOnly.MinValue));

                continue;
            }

            response.Add(new(
                student.Id,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                application.Name,
                consent.ConsentProvided ? ConsentStatusResponse.ConsentStatus.Granted : ConsentStatusResponse.ConsentStatus.Denied,
                DateOnly.FromDateTime(consent.ProvidedAt)));
        }

        return response;
    }
}
