namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.GroupTutorials;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromGroupTutorials
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IGroupTutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public RemoveFromGroupTutorials(
        IStudentRepository studentRepository,
        IGroupTutorialRepository tutorialRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _tutorialRepository = tutorialRepository;
        _logger = logger
            .ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to remove withdrawn Student from Group Tutorials");

            return;
        }

        List<GroupTutorial> tutorials = await _tutorialRepository.GetAllActiveForStudent(notification.StudentId, cancellationToken);

        foreach (GroupTutorial tutorial in tutorials)
        {
            Result unenrol = tutorial.UnenrolStudent(student);

            if (unenrol.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                    .ForContext(nameof(Error), unenrol.Error, true)
                    .Warning("Failed to remove withdrawn Student from Group Tutorials");
            }
        }
    }
}