namespace Constellation.Application.Domains.Hosting.Queries.GetNewsletter;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Models.Hosting.Errors;
using Constellation.Core.Models.Hosting.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetNewsletterQueryHandler
    : IQueryHandler<GetNewsletterQuery, string>
{
    private readonly IHostingRepository _hostingRepository;
    private readonly ILogger _logger;

    public GetNewsletterQueryHandler(
        IHostingRepository hostingRepository,
        ILogger logger)
    {
        _hostingRepository = hostingRepository;
        _logger = logger
            .ForContext<GetNewsletterQuery>();
    }

    public async Task<Result<string>> Handle(GetNewsletterQuery request, CancellationToken cancellationToken)
    {
        Newsletter newsletter = await _hostingRepository.GetNewsletterByIssue(request.Issue, cancellationToken);

        if (newsletter is null)
        {
            _logger
                .ForContext(nameof(GetNewsletterQuery), request, true)
                .ForContext(nameof(Error), NewsletterErrors.NotFound(request.Issue), true)
                .Warning("Newsletter issue {Issue} not found", request.Issue);

            return Result.Failure<string>(NewsletterErrors.NotFound(request.Issue));
        }

        return Result.Success(newsletter.EmbedCode);
    }
}
