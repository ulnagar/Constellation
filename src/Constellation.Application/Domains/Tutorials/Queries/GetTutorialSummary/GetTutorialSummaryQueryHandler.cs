namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialSummary;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.Timetables.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialSummaryQueryHandler
: IQueryHandler<GetTutorialSummaryQuery, TutorialSummaryResponse>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetTutorialSummaryQueryHandler(
        ITutorialRepository tutorialRepository,
        IStaffRepository staffRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
        _logger = logger;
    }

    public async Task<Result<TutorialSummaryResponse>> Handle(GetTutorialSummaryQuery request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(GetTutorialSummaryQuery), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Could not retrieve Tutorial summary");

            return Result.Failure<TutorialSummaryResponse>(TutorialErrors.NotFound(request.TutorialId));
        }

        List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForTutorial(tutorial.Id, cancellationToken);

        List<string> teacherNames = teachers.Select(teacher => teacher.Name.DisplayName).ToList();

        // Calculate minPerFn
        List<PeriodId> periodIds = tutorial.Sessions
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .ToList();

        double minPerFn = await _periodRepository.TotalDurationForCollectionOfPeriods(periodIds, cancellationToken);

        TutorialSummaryResponse entry = new(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate,
            teacherNames,
            (int)minPerFn,
            tutorial.IsCurrent);

        return entry;
    }
}
