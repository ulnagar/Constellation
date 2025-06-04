namespace Constellation.Application.Domains.StaffMembers.Commands.ReinstateStaffMember;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateStaffMemberCommandHandler
    : ICommandHandler<ReinstateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ReinstateStaffMemberCommand>();
    }

    public async Task<Result> Handle(ReinstateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(ReinstateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to reinstate staff member");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        if (!staffMember.IsDeleted)
            return Result.Success();

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(ReinstateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to reinstate staff member with id {Id}", request.StaffId);

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        Result result = staffMember.Reinstate(
            school,
            _dateTime);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ReinstateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate staff member with id {Id}", request.StaffId);

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
