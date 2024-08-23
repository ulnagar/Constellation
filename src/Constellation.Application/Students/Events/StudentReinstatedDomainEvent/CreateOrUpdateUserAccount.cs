namespace Constellation.Application.Students.Events.StudentReinstatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Students.Events;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateUserAccount
    : IDomainEventHandler<StudentReinstatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateOrUpdateUserAccount(
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _logger = logger.ForContext<StudentReinstatedDomainEvent>();
    }

    public async Task Handle(StudentReinstatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentReinstatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to create new Student AppUser");
            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(student.EmailAddress);

        if (user is not null)
        {
            user.IsStudent = true;
            user.StudentId = student.StudentId;

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentReinstatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Student AppUser");

                return;
            }

            return;
        }

        user = new()
        {
            UserName = student.EmailAddress,
            Email = student.EmailAddress,
            FirstName = student.FirstName,
            LastName = student.LastName,
            IsStudent = true,
            StudentId = student.StudentId
        };

        IdentityResult create = await _userManager.CreateAsync(user);

        if (create.Succeeded)
        {
            _logger
                .ForContext(nameof(StudentReinstatedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                .Warning("Failed to create new Student AppUser");
        }
    }
}