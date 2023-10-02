namespace Constellation.Application.Rooms.CreateRoom;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static Constellation.Core.Errors.DomainErrors.LinkedSystems;

internal sealed class CreateRoomCommandHandler
    : ICommandHandler<CreateRoomCommand, RoomResponse>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IAdobeConnectGateway _adobeGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateRoomCommandHandler(
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IAdobeConnectGateway adobeGateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _adobeGateway = adobeGateway;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateRoomCommand>();
    }

    public async Task<Result<RoomResponse>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(CreateRoomCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to create new Adobe Connect Room");

            return Result.Failure<RoomResponse>(OfferingErrors.NotFound(request.OfferingId));
        }

        Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(CreateRoomCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(offering.CourseId), true)
                .Warning("Failed to create new Adobe Connect Room");

            return Result.Failure<RoomResponse>(CourseErrors.NotFound(offering.CourseId));
        }

        Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(CreateRoomCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Faculty.NotFound(course.FacultyId), true)
                .Warning("Failed to create new Adobe Connect Room");

            return Result.Failure<RoomResponse>(DomainErrors.Partners.Faculty.NotFound(course.FacultyId));
        }

        string zeroFillGrade = ((int)course.Grade).ToString().PadLeft(2, '0');

        AdobeConnectRoomDto resource = await _adobeGateway.CreateRoom(
            name: "Aurora College - " + offering.EndDate.Year + " - " + offering.Name,
            year: offering.EndDate.Year.ToString(),
            grade: "Year " + zeroFillGrade,
            dateStart: offering.StartDate.Year + "-" + offering.StartDate.Month.ToString().PadLeft(2, '0') + "-" + offering.StartDate.Day.ToString().PadLeft(2, '0'),
            dateEnd: offering.EndDate.Year + "-" + offering.EndDate.Month.ToString().PadLeft(2, '0') + "-" + offering.EndDate.Day.ToString().PadLeft(2, '0'),
            urlPath: "aurora-" + offering.EndDate.Year + "-" + offering.Name,
            useTemplate: true,
            faculty: faculty.Name,
            detectParentFolder: true,
            parentFolder: ""
        );

        if (resource is null)
        {
            _logger
                .ForContext(nameof(CreateRoomCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.LinkedSystems.AdobeConnect.CannotCreate, true)
                .Warning("Failed to create new Adobe Connect Room");

            return Result.Failure<RoomResponse>(DomainErrors.LinkedSystems.AdobeConnect.CannotCreate);
        }

        AdobeConnectRoom room = new()
        {
            ScoId = resource.ScoId,
            Name = resource.Name,
            UrlPath = resource.UrlPath,
            Protected = resource.Protected
        };

        _unitOfWork.Add(room);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return new RoomResponse(
            resource.ScoId,
            resource.Name,
            resource.UrlPath,
            false);
    }
}
