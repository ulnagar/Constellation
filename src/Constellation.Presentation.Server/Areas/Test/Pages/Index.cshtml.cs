namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        ISentralGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _gateway = gateway;
        _logger = logger;
    }

    public async Task OnGet()
    {
        DateOnly startDate = new DateOnly(2023, 01, 23);
        DateOnly endDate = new DateOnly(2023, 7, 1);

        List<Student> students = await _studentRepository.GetEnrolledForDates(startDate, endDate, default);

        foreach (Student student in students)
        {
            Result<List<DateOnly>> enrolledDates = await _gateway.GetEnrolledDatesForStudent("1277", "2023", startDate, endDate);

            if (enrolledDates.IsFailure)
                continue;

            int enrolledDays = enrolledDates.Value.Count();

            // Get active student enrolments for the period
            // Get offerings from enrolments
            // Get sessions from offerings
            // Get periods from sessions
            // Calculate # classes per day (Stage 3 always 1, Stage 4 & 5 either 1 or 2, Stage 6 max 3)
            // Get whole absences for the student for the period
            // Match absences with days to determine whether it was whole day or part day
            // Take absent whole days from enrolledDays as presentDays
            // Calculate percentage with presentDays / enrolledDays

            // Don't need to determine if a date is week a or week b as the timetable should be symmetrical for number of periods. Need to check that this holds true for Stage 6, as there may have been a P5 class on at random times.
        }

    }
}