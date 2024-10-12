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
        if (request.Responses.Count == 0)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
                .ForContext(nameof(Error), ConsentErrors.Transaction.NoResponses, true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(ConsentErrors.Transaction.NoResponses);
        }

        List<Application> applications = await _consentRepository.GetAllActiveApplications(cancellationToken);

        foreach (KeyValuePair<ApplicationId, bool> entry in request.Responses)
        {
            Application application = applications.FirstOrDefault(application => application.Id == entry.Key);

            if (application is null)
            {
                _logger
                    .ForContext(nameof(CreateTransactionCommand), request, true)
                    .ForContext(nameof(Error), ConsentErrors.Application.NotFound(entry.Key), true)
                    .Warning("Failed to create Consent Transaction");

                return Result.Failure(ConsentErrors.Application.NotFound(entry.Key));
            }

            if (!application.ConsentRequired)
            {
                _logger
                    .ForContext(nameof(CreateTransactionCommand), request, true)
                    .ForContext(nameof(Error), ConsentErrors.Application.NotRequired(application.Id, application.Name), true)
                    .Warning("Failed to create Consent Transaction");

                return Result.Failure(ConsentErrors.Application.NotRequired(application.Id, application.Name));
            }
        }

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        string submissionNotes = request.Notes;

        if (request.SubmissionMethod.Equals(ConsentMethod.PhoneCall))
        {
            submissionNotes += Environment.NewLine;
            submissionNotes += $"Entered by {_currentUserService.EmailAddress}";
        }

        Result<Transaction> transaction = Transaction.Create(
            student.Id,
            request.SubmittedBy,
            _dateTime.Now,
            request.SubmissionMethod,
            submissionNotes,
            request.Responses);

        if (transaction.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), request, true)
                .ForContext(nameof(Error), transaction.Error, true)
                .Warning("Failed to create Consent Transaction");

            return Result.Failure(transaction.Error);
        }

        _consentRepository.Insert(transaction.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
