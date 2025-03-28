namespace Constellation.Application.ThirdPartyConsent.CreateTransaction;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Models.Subjects.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Enums;
using Core.IntegrationEvents;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class CreateTransactionCommandHandler
: ICommandHandler<CreateTransactionCommand>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateTransactionCommandHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger.ForContext<CreateTransactionCommand>();
    }

    public async Task<Result> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        DateTime submittedTime = _dateTime.Now;

        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        ConsentTransactionId transactionId = new();

        List<Transaction.ConsentResponse> transactionConsents = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
            .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(student.Id, cancellationToken);

        List<OfferingId> offeringIds = enrolments.Select(enrolment => enrolment.OfferingId).ToList();

        List<CourseId> courseIds = new();

        foreach (OfferingId offeringId in offeringIds)
        {
            Course course = await _courseRepository.GetByOfferingId(offeringId, cancellationToken);

            if (course is null)
                continue;

            courseIds.Add(course.Id);
        }

        foreach (KeyValuePair<ApplicationId, bool> entry in request.Responses)
        {
            Application application = applications.FirstOrDefault(application => application.Id == entry.Key);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(CreateTransactionCommand), request, true)
                    .ForContext(nameof(Error), ConsentApplicationErrors.NotFound(entry.Key), true)
                    .Warning("Failed to create Consent Transaction");

                return Result.Failure(ConsentApplicationErrors.NotFound(entry.Key));
            }

            if (!application.ConsentRequired)
            {
                _logger
                    .ForContext(nameof(CreateTransactionCommand), request, true)
                    .ForContext(nameof(Error), ConsentApplicationErrors.NotRequired(application.Id, application.Name), true)
                    .Warning("Failed to create Consent Transaction");

                return Result.Failure(ConsentApplicationErrors.NotRequired(application.Id, application.Name));
            }

            List<string> requiredBy = new();

            foreach (var requirement in application.Requirements.Where(requirement => !requirement.IsDeleted))
            {
                switch (requirement)
                {
                    case CourseConsentRequirement courseConsentRequirement:
                        if (courseIds.Contains(courseConsentRequirement.CourseId))
                        {
                            requiredBy.Add($"Course: {courseConsentRequirement.Description}");
                        }

                        break;

                    case GradeConsentRequirement gradeConsentRequirement:
                        if (gradeConsentRequirement.Grade == student.CurrentEnrolment?.Grade)
                        {
                            requiredBy.Add($"Grade: {gradeConsentRequirement.Description}");
                        }

                        break;

                    case StudentConsentRequirement studentConsentRequirement:
                        if (studentConsentRequirement.StudentId == student.Id)
                        {
                            requiredBy.Add($"Student: {studentConsentRequirement.Description}");
                        }

                        break;
                }
            }

            string submissionNotes = request.Notes;

            if (request.SubmissionMethod.Equals(ConsentMethod.PhoneCall))
            {
                submissionNotes += Environment.NewLine;
                submissionNotes += $"Entered by {_currentUserService.EmailAddress}";
            }

            Consent consent = Consent.Create(
                transactionId,
                student.Id,
                application.Id,
                entry.Value,
                request.SubmittedBy,
                submittedTime,
                request.SubmissionMethod,
                submissionNotes);

            application.AddConsentResponse(consent);

            transactionConsents.Add(new(
                application.Id,
                application.Name,
                application.Purpose,
                application.InformationCollected,
                application.StoredCountry,
                application.SharedWith,
                application.ApplicationLink,
                requiredBy,
                entry.Value));
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(request.SubmittedByEmail);

        if (emailAddress.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
                .ForContext(nameof(Error), emailAddress.Error, true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(emailAddress.Error);
        }

        Transaction transaction = Transaction.Create(
            transactionId,
            student.Name,
            student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
            request.SubmittedBy,
            emailAddress.Value,
            submittedTime,
            request.SubmissionMethod,
            request.Notes,
            transactionConsents);

        _consentRepository.Insert(transaction);

        await _unitOfWork.AddIntegrationEvent(new ConsentTransactionReceivedIntegrationEvent(new(), transactionId));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
