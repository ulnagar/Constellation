namespace Constellation.Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;

using Abstractions.Messaging;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRequiredApplicationsForStudentQueryHandler
: IQueryHandler<GetRequiredApplicationsForStudentQuery, List<RequiredApplicationResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetRequiredApplicationsForStudentQueryHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _courseRepository = courseRepository;
        _logger = logger
            .ForContext<GetRequiredApplicationsForStudentQuery>();
    }

    public async Task<Result<List<RequiredApplicationResponse>>> Handle(GetRequiredApplicationsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<RequiredApplicationResponse> responses = new();
        List<ConsentRequirement> requirements = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetRequiredApplicationsForStudentQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve required application consents for student");

            return Result.Failure<List<RequiredApplicationResponse>>(StudentErrors.NotFound(request.StudentId));
        }
        
        List<StudentConsentRequirement> studentRequirements = await _consentRepository.GetRequirementsForStudent(request.StudentId, cancellationToken);

        requirements.AddRange(studentRequirements);

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        List<OfferingId> offeringIds = enrolments.Select(enrolment => enrolment.OfferingId).ToList();

        foreach (OfferingId offeringId in offeringIds)
        {
            Course course = await _courseRepository.GetByOfferingId(offeringId, cancellationToken);

            List<CourseConsentRequirement> courseRequirements = await _consentRepository.GetRequirementsForCourse(course.Id, cancellationToken);

            requirements.AddRange(courseRequirements);
        }

        if (student.CurrentEnrolment is not null)
        {
            List<GradeConsentRequirement> gradeRequirements = await _consentRepository.GetRequirementsForGrade(student.CurrentEnrolment.Grade, cancellationToken);

            requirements.AddRange(gradeRequirements);
        }

        IEnumerable<IGrouping<ApplicationId, ConsentRequirement>> applicationRequirements = requirements.GroupBy(entry => entry.ApplicationId);

        foreach (IGrouping<ApplicationId, ConsentRequirement> applicationId in applicationRequirements)
        {
            Application application = await _consentRepository.GetApplicationById(applicationId.Key, cancellationToken);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(GetRequiredApplicationsForStudentQuery), request, true)
                    .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(applicationId.Key), true)
                    .Warning("Failed to retrieve required application consents for student");

                continue;
            }

            Consent consent = application.Consents
                .Where(consent => consent.StudentId == request.StudentId)
                .MaxBy(consent => consent.ProvidedAt);

            responses.Add(new(
                application.Id,
                application.Name,
                application.Purpose,
                application.InformationCollected.ToList(),
                application.StoredCountry,
                application.SharedWith.ToList(),
                consent?.Id ?? ConsentId.Empty,
                consent?.TransactionId ?? ConsentTransactionId.Empty,
                request.StudentId,
                consent?.ConsentProvided ?? false,
                consent?.ProvidedBy ?? string.Empty,
                consent?.ProvidedAt,
                consent?.Method,
                consent?.MethodNotes ?? string.Empty,
                applicationId.ToDictionary(k => k.Id, k =>
                {
                    string type = k switch
                    {
                        CourseConsentRequirement => "Course",
                        GradeConsentRequirement => "Grade",
                        StudentConsentRequirement => "Student",
                        _ => "Unknown"
                    };

                    return $"{type}: {k.Description}";
                })));
        }

        return responses;
    }
}
