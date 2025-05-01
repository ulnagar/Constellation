namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Errors;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Models.Students.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveAppUser
: IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RemoveAppUser(
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
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
                .Warning("Failed to delete AppUser for withdrawn Student");

            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(student.EmailAddress.Email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(student.EmailAddress), student.EmailAddress)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to delete AppUser for withdrawn Student");

            return;
        }

        IdentityResult update = await _userManager.DeleteAsync(user);

        if (!update.Succeeded)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                .Warning("Failed to delete AppUser for withdrawn Student");
        }
    }
}