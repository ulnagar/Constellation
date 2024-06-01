namespace Constellation.Application.Enrolments.UnenrolStudentFromAllOfferings;

using Abstractions.Messaging;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UnenrolStudentFromAllOfferingsCommandHandler
: ICommandHandler<UnenrolStudentFromAllOfferingsCommand>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UnenrolStudentFromAllOfferingsCommandHandler(
        IEnrolmentRepository enrolmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnenrolStudentFromAllOfferingsCommand request, CancellationToken cancellationToken)
    {
        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            enrolment.Cancel();
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
