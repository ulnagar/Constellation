namespace Constellation.Application.Domains.WorkFlows.Queries.GetCaseById;

using Abstractions.Messaging;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCaseByIdQueryHandler
: IQueryHandler<GetCaseByIdQuery, CaseDetailsResponse>
{
    private readonly ILogger _logger;
    private readonly ICaseRepository _caseRepository;

    public GetCaseByIdQueryHandler(
        ICaseRepository caseRepository,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _logger = logger.ForContext<GetCaseByIdQuery>();
    }

    public async Task<Result<CaseDetailsResponse>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(GetCaseByIdQuery), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Could not retrieve Case for details view");

            return Result.Failure<CaseDetailsResponse>(CaseErrors.NotFound(request.CaseId));
        }

        CaseDetailsResponse response = new(
            item.Id,
            item.ToString(),
            item.Status,
            item.CreatedAt,
            item.Actions
                .Select(action =>
                    new CaseDetailsResponse.CaseActionSummary(
                        action.Id,
                        action.ParentActionId,
                        action.CreatedAt))
                .ToList());

        return response;
    }

}
