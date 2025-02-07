namespace Constellation.Application.Students.Events.StudentEmailAddressChanged;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students.Events;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddOrUpdateCanvasAccount
    :IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddOrUpdateCanvasAccount(
        IStudentRepository studentRepository,
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StudentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to update student ({studentId}) to new email address in Canvas", notification.StudentId);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger.Warning("Could not find student with Id {studentId} to add to Canvas", notification.StudentId);
            return;
        }

        Result<EmailAddress> newAddress = EmailAddress.Create(notification.NewAddress);

        if (newAddress.IsFailure)
        {
            _logger
                .Warning("Could not convert email addresses");
            return;
        }

        if (string.IsNullOrWhiteSpace(notification.OldAddress))
        {
            // No account exists, create a new account
            CreateUserCanvasOperation operation = new(
                student.StudentReferenceNumber.Number,
                student.Name.FirstName,
                student.Name.LastName,
                newAddress.Value.Email.Substring(0, newAddress.Value.Email.IndexOf('@')),
                newAddress.Value.Email);

            _operationsRepository.Insert(operation);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.Information("Scheduled student ({studentId}) addition to Canvas", notification.StudentId);
        }
        else
        {
            UpdateUserEmailCanvasOperation operation = new(
                student.StudentReferenceNumber.Number,
                newAddress.Value.Email.Substring(0, newAddress.Value.Email.IndexOf('@')));

            _operationsRepository.Insert(operation);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.Information("Scheduled student ({studentId}) email address update in Canvas", notification.StudentId);
        }
    }
}