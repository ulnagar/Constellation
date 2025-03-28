namespace Constellation.Application.ThirdPartyConsent.GetConsentsWithFilter;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetConsentsWithFilterQueryHandler
: IQueryHandler<GetConsentsWithFilterQuery, List<ConsentSummaryResponse>>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetConsentsWithFilterQueryHandler(
        IConsentRepository consentRepository,
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetConsentsWithFilterQuery>();
    }

    public async Task<Result<List<ConsentSummaryResponse>>> Handle(GetConsentsWithFilterQuery request, CancellationToken cancellationToken)
    {
        List<ConsentSummaryResponse> response = new();

        List<StudentId> studentIds = new();

        foreach (OfferingId offeringId in request.OfferingIds)
        {
            List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByOfferingId(offeringId, cancellationToken);

            studentIds.AddRange(enrolments.Select(enrolment => enrolment.StudentId));
        }

        foreach (Grade grade in request.Grades)
        {
            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            studentIds.AddRange(students.Select(student => student.Id));
        }

        foreach (string schoolCode in request.SchoolCodes)
        {
            List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(schoolCode, cancellationToken);

            studentIds.AddRange(students.Select(student => student.Id));
        }

        if (request.StudentIds.Count > 0)
        {
            studentIds.AddRange(request.StudentIds);
        }

        studentIds = studentIds
            .Distinct()
            .ToList();

        foreach (var studentId in studentIds)
        {
            Student student = await _studentRepository.GetById(studentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Error), StudentErrors.NotFound(studentId), true)
                    .Warning("Failed to retrieve list of Consent Responses for student");

                continue;
            }

            SchoolEnrolment schoolEnrolment = student.CurrentEnrolment;

            if (schoolEnrolment is null)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                    .Warning("Failed to retrieve list of Consent Responses for student");

                continue;
            }

            List<Application> applications = await _consentRepository.GetApplicationWithConsentForStudent(studentId, cancellationToken);

            foreach (Application application in applications)
            {
                Consent consent = application.Consents
                    .Where(consent => consent.StudentId == studentId)
                    .MaxBy(consent => consent.ProvidedAt);

                if (consent is null)
                {
                    _logger
                        .ForContext(nameof(Student), student, true)
                        .ForContext(nameof(Application), application, true)
                        .ForContext(nameof(Error), ConsentErrors.NoneFoundForStudentAndApplication(application.Name, student.Id), true)
                        .Warning("Failed to retrieve list of Consent Responses for student");

                    continue;
                }

                response.Add(new(
                    student.Name,
                    schoolEnrolment.Grade,
                    schoolEnrolment.SchoolName,
                    consent.Id,
                    DateOnly.FromDateTime(consent.ProvidedAt),
                    application.Name,
                    consent.ConsentProvided));
            }
        }

        return response;
    }
}
