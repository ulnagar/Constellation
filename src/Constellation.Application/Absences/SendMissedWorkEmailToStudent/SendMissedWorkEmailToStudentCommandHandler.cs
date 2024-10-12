namespace Constellation.Application.Absences.SendMissedWorkEmailToStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Core.Models.Students.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendMissedWorkEmailToStudentCommandHandler
    : ICommandHandler<SendMissedWorkEmailToStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendMissedWorkEmailToStudentCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SendMissedWorkEmailToStudentCommand>();
    }

    public async Task<Result> Handle(SendMissedWorkEmailToStudentCommand request, CancellationToken cancellationToken) 
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        foreach (var family in families)
        {
            List<EmailRecipient> recipients = new();

            Result<EmailRecipient> studentEmailResult = EmailRecipient.Create(student.Name.DisplayName, student.EmailAddress.Email);

            if (studentEmailResult.IsFailure)
            {
                _logger
                    .ForContext("Error", studentEmailResult.Error, true)
                    .Warning("{jobId}: Could not create email recipient from student {name}", request.JobId, student.Name.DisplayName);

                return Result.Failure(studentEmailResult.Error);
            }

            recipients.Add(studentEmailResult.Value);

            Result<EmailRecipient> familyEmail = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

            if (familyEmail.IsSuccess)
                recipients.Add(familyEmail.Value);

            foreach (Parent parent in family.Parents)
            {
                Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

                if (nameResult.IsFailure)
                    continue;

                Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);
            Course course = offering is not null ? await _courseRepository.GetById(offering.CourseId, cancellationToken) : null;

            await _emailService.SendMissedWorkEmail(
                student,
                course is not null ? course.Name : string.Empty,
                offering is not null ? offering.Name : string.Empty,
                request.AbsenceDate,
                recipients,
                cancellationToken);
        }

        return Result.Success();
    }
}
