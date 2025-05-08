namespace Constellation.Application.Domains.Edval.Queries.GetEdvalDifferences;

using Abstractions.Messaging;
using Core.Models.Edval;
using Core.Shared;
using Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetEdvalDifferencesQueryHandler
: IQueryHandler<GetEdvalDifferencesQuery, List<Difference>>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly ILogger _logger;

    public GetEdvalDifferencesQueryHandler(
        IEdvalRepository edvalRepository,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _logger = logger
            .ForContext<GetEdvalDifferencesQuery>();
    }

    public async Task<Result<List<Difference>>> Handle(GetEdvalDifferencesQuery request, CancellationToken cancellationToken)
    {
        return await _edvalRepository.GetDifferences(cancellationToken);
    }
}
