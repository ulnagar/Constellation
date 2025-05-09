﻿namespace Constellation.Application.Domains.Students.Events.StudentEmailAddressChanged;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Events;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddOrUpdateUserAccount
: IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public AddOrUpdateUserAccount(
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _logger = logger
            .ForContext<StudentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to update Student AppUser for new Email");
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
            // No previous address known. Create new user
            AppUser user = new()
            {
                UserName = student.EmailAddress.Email,
                Email = student.EmailAddress.Email,
                FirstName = student.Name.PreferredName,
                LastName = student.Name.LastName,
                IsStudent = true,
                StudentId = student.Id
            };

            IdentityResult create = await _userManager.CreateAsync(user);

            if (create.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                    .Warning("Failed to update Student AppUser for new Email");
            }
        }
        else
        {
            AppUser user = await _userManager.FindByEmailAsync(notification.OldAddress);

            if (user is not null)
            {
                user.IsStudent = true;
                user.StudentId = student.Id;
                user.UserName = newAddress.Value.Email;
                user.Email = newAddress.Value.Email;

                IdentityResult update = await _userManager.UpdateAsync(user);

                if (!update.Succeeded)
                {
                    _logger
                        .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Student AppUser for new Email");
                }

                return;
            }

            user = await _userManager.FindByEmailAsync(newAddress.Value.Email);

            if (user is not null)
            {
                user.IsStudent = true;
                user.StudentId = student.Id;

                IdentityResult update = await _userManager.UpdateAsync(user);

                if (!update.Succeeded)
                {
                    _logger
                        .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Student AppUser for new Email");
                }

                return;
            }

            user = new()
            {
                UserName = student.EmailAddress.Email,
                Email = student.EmailAddress.Email,
                FirstName = student.Name.PreferredName,
                LastName = student.Name.LastName,
                IsStudent = true,
                StudentId = student.Id
            };

            IdentityResult create = await _userManager.CreateAsync(user);

            if (create.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                    .Warning("Failed to update Student AppUser for new Email");
            }
        }
    }
}