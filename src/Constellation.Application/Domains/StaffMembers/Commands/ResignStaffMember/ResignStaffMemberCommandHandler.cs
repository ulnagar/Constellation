namespace Constellation.Application.Domains.StaffMembers.Commands.ResignStaffMember;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ResignStaffMemberCommandHandler
: ICommandHandler<ResignStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ResignStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResignStaffMemberCommand>();
    }

    public async Task<Result> Handle(ResignStaffMemberCommand request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(ResignStaffMemberCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to mark Staff Member resigned");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        staffMember.Resign(_dateTime);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
