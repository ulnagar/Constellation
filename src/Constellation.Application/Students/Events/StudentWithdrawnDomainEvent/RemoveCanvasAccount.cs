namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Core.Models.Operations;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveCanvasAccount
    :IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveCanvasAccount(
        IStudentRepository studentRepository,
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to remove student ({studentId}) from Canvas", notification.StudentId);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Could not find student with Id {studentId} to remove from Canvas", notification.StudentId);

            return;
        }
       
        DeleteUserCanvasOperation operation = new(student.StudentReferenceNumber.Number);

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) removal from Canvas", notification.StudentId);
    }
}