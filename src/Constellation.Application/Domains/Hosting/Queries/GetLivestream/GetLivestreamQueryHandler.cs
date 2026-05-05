namespace Constellation.Application.Domains.Hosting.Queries.GetLivestream;

using Abstractions.Messaging;
using Constellation.Application.Domains.Hosting.Queries.GetNewsletter;
using Constellation.Core.Models.Hosting.Errors;
using Core.Models.Hosting;
using Core.Models.Hosting.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetLivestreamQueryHandler
: IQueryHandler<GetLivestreamQuery, Livestream>
{
    private readonly IHostingRepository _hostingRepository;
    private readonly ILogger _logger;

    public GetLivestreamQueryHandler(
        IHostingRepository hostingRepository,
        ILogger logger)
    {
        _hostingRepository = hostingRepository;
        _logger = logger
            .ForContext<GetLivestreamQuery>();
    }

    public async Task<Result<Livestream>> Handle(GetLivestreamQuery request, CancellationToken cancellationToken)
    {
        Livestream? livestream = await _hostingRepository.GetLivestreamById(request.Id, cancellationToken);

        if (livestream is null)
        {
            _logger
                .ForContext(nameof(GetNewsletterQuery), request, true)
                .ForContext(nameof(Error), LivestreamErrors.NotFound(request.Id), true)
                .Warning("Livestream with Id '{Id}' not found", request.Id);

            return Result.Failure<Livestream>(LivestreamErrors.NotFound(request.Id));
        }

        return livestream;
    }
}
