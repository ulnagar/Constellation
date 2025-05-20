namespace Constellation.Application.Domains.Edval.Commands.RemoveIgnoreRuleForDifference;

using Abstractions.Messaging;
using Constellation.Application.Domains.Edval.Commands.IgnoreEdvalDifference;
using Constellation.Application.Domains.Edval.Repositories;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Edval.Errors;
using Constellation.Core.Models.Edval;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

internal sealed class RemoveIgnoreRuleForDifferenceCommandHandler
: ICommandHandler<RemoveIgnoreRuleForDifferenceCommand>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveIgnoreRuleForDifferenceCommandHandler(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveIgnoreRuleForDifferenceCommand>();
    }

    public async Task<Result> Handle(RemoveIgnoreRuleForDifferenceCommand request, CancellationToken cancellationToken)
    {
        Difference originalDifference = await _edvalRepository.GetDifference(request.DifferenceId, cancellationToken);

        if (originalDifference is null)
        {
            _logger
                .ForContext(nameof(IgnoreEdvalDifferenceCommand), request, true)
                .ForContext(nameof(Error), DifferenceErrors.NotFound(request.DifferenceId), true)
                .Warning("Failed to remove Ignore Record for Edval Difference");

            return Result.Failure(DifferenceErrors.NotFound(request.DifferenceId));
        }

        List<EdvalIgnore> ignoreRecords = await _edvalRepository.GetIgnoreRecords(originalDifference.Type, cancellationToken);

        ignoreRecords = ignoreRecords.Where(record =>
                record.System == originalDifference.System && 
                record.Identifier == originalDifference.Identifier)
            .ToList();

        foreach (EdvalIgnore ignore in ignoreRecords)
            _edvalRepository.Remove(ignore);
        
        List<Difference> differences = await _edvalRepository.GetDifferences(cancellationToken);
        differences = differences.Where(difference =>
                difference.Type == originalDifference.Type &&
                difference.System == originalDifference.System &&
                difference.Identifier == originalDifference.Identifier)
            .ToList();

        foreach (var difference in differences)
            difference.SetIgnored(false);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
