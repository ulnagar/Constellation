namespace Constellation.Application.Offerings.RemoveTeacherFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromAllOfferingsCommandHandler
    : ICommandHandler<RemoveTeacherFromAllOfferingsCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeacherFromAllOfferingsCommandHandler(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveTeacherFromAllOfferingsCommand>();
    }

    public async Task<Result> Handle(RemoveTeacherFromAllOfferingsCommand request, CancellationToken cancellationToken)
    {
        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        foreach (Offering offering in offerings)
        {
            List<TeacherAssignment> assignments = offering
                .Teachers
                .Where(assignment =>
                    assignment.StaffId == request.StaffId &&
                    !assignment.IsDeleted)
                .ToList();

            foreach (TeacherAssignment assignment in assignments)
            {
                offering.RemoveTeacher(request.StaffId, assignment.Type);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
