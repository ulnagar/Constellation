namespace Constellation.Application.Domains.Hosting.Commands.UpsertLivestream;

using Abstractions.Messaging;
using Constellation.Core.Models.Hosting;
using Core.Models.Hosting.Errors;
using Core.Models.Hosting.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpsertLivestreamCommandHandler
: ICommandHandler<UpsertLivestreamCommand>
{
    private readonly IHostingRepository _hostingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertLivestreamCommandHandler(
        IHostingRepository hostingRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _hostingRepository = hostingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpsertLivestreamCommand>();
    }

    public async Task<Result> Handle(UpsertLivestreamCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            Livestream? existingLivestream = await _hostingRepository.GetLivestreamById(request.Id.Value, cancellationToken);

            if (existingLivestream is null)
            {
                _logger
                    .ForContext(nameof(UpsertLivestreamCommand), request, true)
                    .ForContext(nameof(Error), LivestreamErrors.NotFound(request.Id.Value), true)
                    .Warning("Failed to update Livestream");

                return Result.Failure(LivestreamErrors.NotFound(request.Id.Value));
            }

            Result update = existingLivestream.Update(request.Name,
                request.EmbedCode,
                request.Description,
                request.StartsOn,
                request.ExpiresOn);
        }
        else
        {
            Result<Livestream> livestream = Livestream.Create(
                request.Name,
                request.EmbedCode,
                request.Description,
                request.StartsOn,
                request.ExpiresOn);

            if (livestream.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpsertLivestreamCommand), request, true)
                    .ForContext(nameof(Error), livestream.Error, true)
                    .Warning("Failed to create Livestream");

                return Result.Failure(livestream.Error);
            }

            _hostingRepository.Insert(livestream.Value);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
