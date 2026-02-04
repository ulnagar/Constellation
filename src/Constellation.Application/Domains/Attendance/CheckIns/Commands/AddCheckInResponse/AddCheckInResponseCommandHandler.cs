namespace Constellation.Application.Domains.Attendance.CheckIns.Commands.AddCheckInResponse;

using Abstractions.Messaging;
using Core.Models.Attendance.Checkin;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Threading.Tasks;

internal sealed class AddCheckInResponseCommandHandler
: ICommandHandler<AddCheckInResponseCommand>
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddCheckInResponseCommandHandler(
        ICheckInRepository checkInRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _checkInRepository = checkInRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddCheckInResponseCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> studentEmail = EmailAddress.Create(request.Response.EmailAddress);

        if (studentEmail.IsFailure)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), studentEmail.Error, true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(studentEmail.Error);
        }

        Student? student = await _studentRepository.GetCurrentByEmailAddress(studentEmail.Value, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFoundByEmail(studentEmail.Value), true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(StudentErrors.NotFoundByEmail(studentEmail.Value));
        }

        List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent(student.Id), true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(OfferingErrors.NotFoundForStudent(student.Id));
        }

        Offering? matchedOffering = null;

        if (request.Response.Group == GroupOption.Stage3)
        {
            if (offerings.Count > 1)
            {
                _logger
                    .ForContext(nameof(AddCheckInResponseCommand), request, true)
                    .ForContext(nameof(Error), OfferingErrors.TooManyForStudent(student.Id), true)
                    .Warning("Failed to convert form response to Attendance Check In Response");

                return Result.Failure(OfferingErrors.TooManyForStudent(student.Id));
            }

            matchedOffering = offerings.First();
        } 
        else if (request.Response.Group == GroupOption.Stage6)
        {
            foreach (var offering in offerings)
            {
                Course? offeringCourse = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                if (offeringCourse is null)
                    continue;

                if (offeringCourse.Name == request.Response.Subject)
                {
                    matchedOffering = offering;
                    break;
                }
            }
        }
        else
        {
            string? code = request.Response.Group switch
            {
                GroupOption.English => "ENG",
                GroupOption.Mathematics => "MAT",
                GroupOption.Science => "SCI",
                _ => null
            };

            if (code is null)
            {
                _logger
                    .ForContext(nameof(AddCheckInResponseCommand), request, true)
                    .ForContext(nameof(Error), OfferingErrors.NoneFound, true)
                    .Warning("Failed to convert form response to Attendance Check In Response");

                return Result.Failure(OfferingErrors.NoneFound);
            }

            Offering? offering = offerings.FirstOrDefault(entry => entry.Name.Value.Contains(code));

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(AddCheckInResponseCommand), request, true)
                    .ForContext(nameof(Error), OfferingErrors.NotFoundForName(code), true)
                    .Warning("Failed to convert form response to Attendance Check In Response");

                return Result.Failure(OfferingErrors.NotFoundForName(code));
            }

            matchedOffering = offering;
        }

        if (matchedOffering is null)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NoneFound, true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(OfferingErrors.NoneFound);
        }

        Course? course = await _courseRepository.GetById(matchedOffering.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(matchedOffering.CourseId), true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(CourseErrors.NotFound(matchedOffering.CourseId));
        }

        if (request.Response.Submitted is null)
        {
            _logger
                .ForContext(nameof(AddCheckInResponseCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(matchedOffering.CourseId), true)
                .Warning("Failed to convert form response to Attendance Check In Response");

            return Result.Failure(CourseErrors.NotFound(matchedOffering.CourseId));
        }

        CheckInResponse item = new(
            student,
            matchedOffering,
            course,
            request.Response.Submitted.Value,
            request.Response.Sentiment);

        _checkInRepository.Insert(item);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
