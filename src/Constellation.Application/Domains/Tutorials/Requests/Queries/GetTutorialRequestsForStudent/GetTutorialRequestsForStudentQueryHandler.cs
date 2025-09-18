namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestsForStudent;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialRequestsForStudentQueryHandler
: IQueryHandler<GetTutorialRequestsForStudentQuery, List<TutorialRequestResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetTutorialRequestsForStudentQueryHandler(
        ITutorialRepository tutorialRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _periodRepository = periodRepository;
        _logger = logger
            .ForContext<GetTutorialRequestsForStudentQuery>();
    }

    public async Task<Result<List<TutorialRequestResponse>>> Handle(GetTutorialRequestsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<TutorialRequestResponse> responses = [];

        List<Request> tutorialRequests = await _tutorialRepository.GetRequestsForStudent(request.StudentId, cancellationToken);

        foreach (Request tutorialRequest in tutorialRequests)
        {
            List<Period> periods = await _periodRepository.GetListFromIds(tutorialRequest.PeriodIds.ToList(), cancellationToken);

            TutorialRequestResponse response = new(
                tutorialRequest.Id,
                tutorialRequest.StudentId,
                tutorialRequest.Student,
                tutorialRequest.Grade,
                tutorialRequest.School,
                tutorialRequest.Type,
                tutorialRequest.Subject,
                periods,
                tutorialRequest.Justification,
                tutorialRequest.Status,
                tutorialRequest.ReviewedBy,
                tutorialRequest.ReviewedAt,
                tutorialRequest.Notes);

            responses.Add(response);
        }

        return responses;
    }
}
