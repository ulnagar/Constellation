namespace Constellation.Application.Domains.Edval.Queries.CountEdvalDifferences;

using Abstractions.Messaging;
using Core.Shared;
using Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountEdvalDifferencesQueryHandler
: IQueryHandler<CountEdvalDifferencesQuery, int>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly ILogger _logger;

    public CountEdvalDifferencesQueryHandler(
        IEdvalRepository edvalRepository,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _logger = logger
            .ForContext<CountEdvalDifferencesQuery>();
    }

    public async Task<Result<int>> Handle(CountEdvalDifferencesQuery request, CancellationToken cancellationToken)
    {
        return await _edvalRepository.CountDifferences(cancellationToken);
    }
}
