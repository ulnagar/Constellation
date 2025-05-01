namespace Constellation.Application.Domains.Students.Events.StudentReinstatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Events;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateCanvasAccount
    : IDomainEventHandler<StudentReinstatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCanvasAccount(
        IStudentRepository studentRepository,
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentReinstatedDomainEvent>();
    }

    public async Task Handle(StudentReinstatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add student ({studentId}) to Canvas", notification.StudentId);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger.Warning("Could not find student with Id {studentId} to add to Canvas", notification.StudentId);
            return;
        }

        if (student.StudentReferenceNumber == StudentReferenceNumber.Empty)
        {
            _logger.Warning("Student with id {StudentId} does not have a valid SRN to add to Canvas", notification.StudentId);
            return;
        }

        if (student.EmailAddress == EmailAddress.None)
        {
            _logger.Warning("Student with id {StudentId} does not have a valid email address to add to Canvas", notification.StudentId);
            return;
        }

        CreateUserCanvasOperation operation = new(
            student.StudentReferenceNumber.Number,
            student.Name.PreferredName,
            student.Name.LastName,
            student.EmailAddress.Email.Substring(0, student.EmailAddress.Email.IndexOf('@')),
            student.EmailAddress.Email);

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) addition to Canvas", notification.StudentId);
    }
}