namespace Constellation.Application.ThirdPartyConsent.CreateTransaction;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateTransactionCommandHandler(
        IConsentRepository consentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger.ForContext<CreateTransactionCommand>();
    }

    public async Task<Result> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        ConsentTransactionId transactionId = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
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
                    .ForContext(nameof(Error), ConsentApplicationErrors.NotRequired(application.Id, application.Name),
                        true)
                    .Warning("Failed to create Consent Transaction");

                return Result.Failure(ConsentApplicationErrors.NotRequired(application.Id, application.Name));
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
                _dateTime.Now,
                request.SubmissionMethod,
                submissionNotes);

            application.AddConsentResponse(consent);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
