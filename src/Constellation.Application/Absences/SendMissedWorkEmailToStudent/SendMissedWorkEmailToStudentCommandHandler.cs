﻿namespace Constellation.Application.Absences.SendMissedWorkEmailToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendMissedWorkEmailToStudentCommandHandler
    : ICommandHandler<SendMissedWorkEmailToStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendMissedWorkEmailToStudentCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SendMissedWorkEmailToStudentCommand>();
    }

    public async Task<Result> Handle(SendMissedWorkEmailToStudentCommand request, CancellationToken cancellationToken) 
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
        List<Parent> parents = families.SelectMany(family => family.Parents).ToList();

        List<string> numbers = parents
            .Select(parent => parent.MobileNumber)
            .Distinct()
            .ToList();

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> studentEmailResult = EmailRecipient.Create(student.GetName()?.DisplayName, student.EmailAddress);

        if (studentEmailResult.IsFailure)
        {
            _logger
                .ForContext("Error", studentEmailResult.Error, true)
                .Warning("{jobId}: Could not create email recipient from student {name}", request.JobId, student.GetName()?.DisplayName);

            return Result.Failure(studentEmailResult.Error);
        }

        recipients.Add(studentEmailResult.Value);

        foreach (Family family in families)
        {
            Result<EmailRecipient> result = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

            if (result.IsSuccess)
                recipients.Add(result.Value);
        }

        foreach (Parent parent in parents)
        {
            Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

            if (nameResult.IsFailure)
                continue;

            Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

            if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                recipients.Add(result.Value);
        }

        await _emailService.SendMissedWorkEmail(student, recipients, cancellationToken);

        return Result.Success();
    }
}