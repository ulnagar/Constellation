namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using BaseModels;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables.Repositories;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ISentralGateway _gateway;
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
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;
        _gateway = gateway;

        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task OnGet()
    {
        await _gateway.IssueAward();
    }
}