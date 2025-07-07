namespace Constellation.Application.Domains.StaffMembers.Commands.RemoveSchoolAssignment;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSchoolAssignmentCommandHandler
: ICommandHandler<RemoveSchoolAssignmentCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveSchoolAssignmentCommandHandler(
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveSchoolAssignmentCommand request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(RemoveSchoolAssignmentCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to remove School Assignment");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        SchoolAssignment assignment = staffMember.SchoolAssignments.FirstOrDefault(entry => entry.Id == request.AssignmentId);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(RemoveSchoolAssignmentCommand), request, true)
                .ForContext(nameof(Error), SchoolAssignmentErrors.NotFound(request.AssignmentId), true)
                .Warning("Failed to remove School Assignment");

            return Result.Failure(SchoolAssignmentErrors.NotFound(request.AssignmentId));
        }

        staffMember.RemoveSchoolAssignment(assignment, _dateTime);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
