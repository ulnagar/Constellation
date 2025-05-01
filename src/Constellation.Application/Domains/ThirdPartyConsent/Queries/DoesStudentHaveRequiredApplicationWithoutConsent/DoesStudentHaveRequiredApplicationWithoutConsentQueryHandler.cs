namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.DoesStudentHaveRequiredApplicationWithoutConsent;

using Abstractions.Messaging;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Errors;
using Constellation.Core.Models.ThirdPartyConsent.Repositories;
using Core.Models.Enrolments;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class DoesStudentHaveRequiredApplicationWithoutConsentQueryHandler
    : IQueryHandler<DoesStudentHaveRequiredApplicationWithoutConsentQuery, bool>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public DoesStudentHaveRequiredApplicationWithoutConsentQueryHandler(
        IStudentRepository studentRepository,
        IConsentRepository consentRepository,
        IEnrolmentRepository enrolmentRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _consentRepository = consentRepository;
        _enrolmentRepository = enrolmentRepository;
        _courseRepository = courseRepository;
        _logger = logger
            .ForContext<DoesStudentHaveRequiredApplicationWithoutConsentQuery>();
    }

    public async Task<Result<bool>> Handle(DoesStudentHaveRequiredApplicationWithoutConsentQuery request, CancellationToken cancellationToken)
    {
        List<ConsentRequirement> requirements = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);
        if (student is null)
        {
            _logger
                .ForContext(nameof(DoesStudentHaveRequiredApplicationWithoutConsentQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
            .Warning("Failed to retrieve required application consents for student");

            return Result.Failure<bool>(StudentErrors.NotFound(request.StudentId));
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
                    .ForContext(nameof(DoesStudentHaveRequiredApplicationWithoutConsentQuery), request, true)
                    .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(applicationId.Key), true)
                    .Warning("Failed to retrieve required application consents for student");

                continue;
            }

            if (!application.ConsentRequired || application.IsDeleted)
                continue;

            Consent consent = application.Consents
                .Where(consent => consent.StudentId == request.StudentId)
                .MaxBy(consent => consent.ProvidedAt);

            if (consent is null)
                return true;
        }

        return false;
    }
}
