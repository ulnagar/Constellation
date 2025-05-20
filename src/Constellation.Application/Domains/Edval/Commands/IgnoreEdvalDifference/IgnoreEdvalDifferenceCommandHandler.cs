namespace Constellation.Application.Domains.Edval.Commands.IgnoreEdvalDifference;

using Abstractions.Messaging;
using Core.Models.Edval;
using Core.Models.Edval.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class IgnoreEdvalDifferenceCommandHandler
: ICommandHandler<IgnoreEdvalDifferenceCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public IgnoreEdvalDifferenceCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IgnoreEdvalDifferenceCommand>();
    }

    public async Task<Result> Handle(IgnoreEdvalDifferenceCommand request, CancellationToken cancellationToken)
    {
        Difference originalDifference = await _edvalRepository.GetDifference(request.DifferenceId, cancellationToken);

        if (originalDifference is null)
        {
            _logger
                .ForContext(nameof(IgnoreEdvalDifferenceCommand), request, true)
                .ForContext(nameof(Error), DifferenceErrors.NotFound(request.DifferenceId), true)
                .Warning("Failed to create Ignore Record for Edval Difference");

            return Result.Failure(DifferenceErrors.NotFound(request.DifferenceId));
        }

        EdvalIgnore ignore = originalDifference.CreateIgnoreRecord();

        _edvalRepository.Insert(ignore);

        List<Difference> differences = await _edvalRepository.GetDifferences(cancellationToken);
        differences = differences.Where(difference =>
                difference.Type == originalDifference.Type &&
                difference.System == originalDifference.System &&
                difference.Identifier == originalDifference.Identifier)
            .ToList();

        foreach (var difference in differences)
            difference.SetIgnored(true);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
