namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.CreateEnrolmentApplication;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentParent.Repositories;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using Interfaces;
using Serilog;

internal sealed class CreateEnrolmentApplicationCommandHandler
: ICommandHandler<CreateEnrolmentApplicationCommand>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly IEnrolmentParentRepository _parentRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public CreateEnrolmentApplicationCommandHandler(
        IEnrolmentApplicationRepository repository,
        IEnrolmentStudentRepository studentRepository,
        IEnrolmentParentRepository parentRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _parentRepository = parentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<CreateEnrolmentApplicationCommand>();
    }

    public async Task<Result> Handle(CreateEnrolmentApplicationCommand request, CancellationToken cancellationToken)
    {
        EnrolmentStudent? existingStudent = await GetExistingStudent(
            request.StudentEmailAddress,
            request.StudentReferenceNumber, 
            cancellationToken);
        
        if (existingStudent is null)
        {
            existingStudent = EnrolmentStudent.Create(
                request.StudentName,
                request.StudentGender,
                request.StudentReferenceNumber,
                request.DateOfBirth,
                request.StudentEmailAddress);

            _studentRepository.Insert(existingStudent);
        }

        EnrolmentParent? existingParent = await GetExistingParent(
            request.ParentEmailAddress,
            cancellationToken);

        if (existingParent is null && request.ParentName is not null)
        {
            existingParent = EnrolmentParent.Create(
                null,
                request.ParentName,
                request.ParentEmailAddress,
                request.ParentPhoneNumber,
                request.MailingAddress);

            _parentRepository.Insert(existingParent);
        }

        if (existingParent is null)
            return Result.Failure(EnrolmentApplicationErrors.ParentRequired);

        Result<Application> application = Application.Create(
            request.PeriodId,
            existingStudent.Id,
            existingParent,
            request.ApplicationReference,
            request.CurrentSchoolCode,
            request.CurrentSchool,
            request.DestinationSchoolCode,
            request.DestinationSchool,
            request.Program,
            request.Grade);

        if (application.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateEnrolmentApplicationCommand), request, true)
                .ForContext(nameof(Error), application.Error, true)
                .Warning("Failed to create new Enrolment Application");

            return application;
        }

        foreach (CourseSelection course in request.Courses)
        {
            if (string.IsNullOrWhiteSpace(course.Line))
                return Result.Failure(EnrolmentApplicationErrors.CourseMustIncludeLine(course.Course));

            application.Value.AddCourse(course.Course, course.Line);
        }

        _repository.Insert(application.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<EnrolmentStudent?> GetExistingStudent(
        EmailAddress? email, 
        StudentReferenceNumber? srn,
        CancellationToken cancellationToken = default)
    {
        if (email is not null)
        {
            EnrolmentStudent? existingStudent = await _studentRepository.GetByEmail(email, cancellationToken);

            if (existingStudent is not null)
                return existingStudent;
        }

        if (srn is not null)
        {

            EnrolmentStudent? existingStudent = await _studentRepository.GetBySRN(srn, cancellationToken);

            if (existingStudent is not null)
                return existingStudent;
        }

        return null;
    }

    private async Task<EnrolmentParent?> GetExistingParent(
        EmailAddress? email,
        CancellationToken cancellationToken = default)
    {
        Guid? userId = await _currentUserService.GetUserId();

        if (userId is not null)
        {
            EnrolmentParentId enrolmentParentId = EnrolmentParentId.FromValue(userId.Value);

            EnrolmentParent? existingParent = await _parentRepository.GetById(enrolmentParentId, cancellationToken);

            if (existingParent is not null) 
                return existingParent;
        }

        if (email is not null)
        {
            EnrolmentParent? existingParent = await _parentRepository.GetByEmail(email, cancellationToken);

            if (existingParent is not null)
                return existingParent;
        }

        return null;
    }
}
