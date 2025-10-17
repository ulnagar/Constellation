namespace Constellation.Application.Domains.StaffMembers.Commands.TransferStaffMember;

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

internal sealed class TransferStaffMemberCommandHandler : ICommandHandler<TransferStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public TransferStaffMemberCommandHandler(
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
        _logger = logger
            .ForContext<TransferStaffMemberCommand>();
    }

    public async Task<Result> Handle(TransferStaffMemberCommand request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TransferStaffMemberCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to transfer Staff Member to new school");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(TransferStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to transfer Staff Member to new school or grade");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        Result newEnrolment = staffMember.AddSchoolAssignment(
            school.Code,
            school.Name,
            _dateTime,
            request.StartDate);

        if (newEnrolment.IsFailure)
        {
            _logger
                .ForContext(nameof(TransferStaffMemberCommand), request, true)
                .ForContext(nameof(Error), newEnrolment.Error, true)
                .Warning("Failed to transfer Staff Member to new school");

            return Result.Failure(newEnrolment.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}