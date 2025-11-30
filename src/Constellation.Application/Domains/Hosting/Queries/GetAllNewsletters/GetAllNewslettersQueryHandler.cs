namespace Constellation.Application.Domains.Hosting.Queries.GetAllNewsletters;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Models.Hosting.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllNewslettersQueryHandler
    : IQueryHandler<GetAllNewslettersQuery, List<Newsletter>>
{
    private readonly IHostingRepository _hostingRepository;
    private readonly ILogger _logger;

    public GetAllNewslettersQueryHandler(
        IHostingRepository hostingRepository,
        ILogger logger)
    {
        _hostingRepository = hostingRepository;
        _logger = logger
            .ForContext<GetAllNewslettersQuery>();
    }

    public async Task<Result<List<Newsletter>>> Handle(GetAllNewslettersQuery request, CancellationToken cancellationToken)
    {
        List<Newsletter> newsletters = await _hostingRepository.GetAllNewsletters(cancellationToken);

        return newsletters;
    }
}
