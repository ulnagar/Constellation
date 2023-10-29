namespace Constellation.Application.Students.Events.StudentReinstatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Events;
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

        CreateUserCanvasOperation operation = new()
        {
            UserId = student.StudentId,
            FirstName = student.FirstName,
            LastName = student.LastName,
            PortalUsername = student.PortalUsername,
            EmailAddress = student.EmailAddress
        };

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) addition to Canvas", notification.StudentId);
    }
}