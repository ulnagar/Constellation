namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IAttendancePlanRepository _attendancePlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;

        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task OnGet()
    {
        OfferingId offeringId = OfferingId.FromValue(new Guid("1c1e9a19-c54e-4bef-9306-36b5f73cd74f"));

        await _periodRepository.GetByDayAndOfferingId(1, offeringId);
    }
}