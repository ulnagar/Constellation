namespace Constellation.Application.Offerings.RemoveSession;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSessionCommandHandler
    : ICommandHandler<RemoveSessionCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveSessionCommandHandler(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveSessionCommand>();
    }

    public async Task<Result> Handle(RemoveSessionCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find Offering with Id {id}", request.OfferingId);

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        Session session = offering.Sessions.FirstOrDefault(session => session.Id == request.SessionId);

        if (session is null)
        {
            _logger.Warning("Could not find Session with Id {id}", request.SessionId);

            return Result.Failure(SessionErrors.NotFound(request.SessionId));
        }

        offering.DeleteSession(session.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
