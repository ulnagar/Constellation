namespace Constellation.Application.Domains.Hosting.Commands.UpsertNewsletter;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Models.Hosting.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertNewsletterCommandHandler
    : ICommandHandler<UpsertNewsletterCommand>
{
    private readonly IHostingRepository _hostingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertNewsletterCommandHandler(
        IHostingRepository hostingRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _hostingRepository = hostingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpsertNewsletterCommand request, CancellationToken cancellationToken)
    {
        Newsletter? existingNewsletter = await _hostingRepository.GetNewsletterByIssue(request.Issue, cancellationToken);

        if (existingNewsletter is null)
        {
            var newsletter = Newsletter.Create(
                request.Issue,
                request.Name,
                request.EmbedCode);

            if (newsletter.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpsertNewsletterCommand), request, true)
                    .ForContext(nameof(Error), newsletter.Error, true)
                    .Warning("Failed to create Newsletter");

                return Result.Failure(newsletter.Error);
            }

            _hostingRepository.Insert(newsletter.Value);
        }
        else
        {
            var update = existingNewsletter.Update(
                request.Name,
                request.EmbedCode);

            if (update.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpsertNewsletterCommand), request, true)
                    .ForContext(nameof(Newsletter), existingNewsletter, true)
                    .ForContext(nameof(Error), update.Error, true)
                    .Warning("Failed to update Newsletter");

                return Result.Failure(update.Error);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
