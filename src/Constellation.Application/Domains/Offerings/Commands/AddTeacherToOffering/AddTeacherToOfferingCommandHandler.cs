namespace Constellation.Application.Domains.Offerings.Commands.AddTeacherToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToOfferingCommandHandler
    : ICommandHandler<AddTeacherToOfferingCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeacherToOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddTeacherToOfferingCommand>();
    }

    public async Task<Result> Handle(AddTeacherToOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find Offering with Id {id}", request.OfferingId);

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        offering.AddTeacher(request.StaffId, request.AssignmentType);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
