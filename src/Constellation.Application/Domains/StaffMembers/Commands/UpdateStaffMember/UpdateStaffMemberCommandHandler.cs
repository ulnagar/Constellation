namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMember;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStaffMemberCommandHandler
: ICommandHandler<UpdateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStaffMemberCommandHandler(
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
        _logger = logger.ForContext<UpdateStaffMemberCommand>();
    }

    //TODO : R1.18: Create StaffMemberEmailAddressChangedDomainEvent handler to update user account, Canvas, Teams, etc.

    public async Task<Result> Handle(UpdateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to update Staff Member");

            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        Result<Name> name = Name.Create(request.FirstName, request.PreferredName, request.LastName);

        if (name.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to update Staff Member");

            return Result.Failure(name.Error);
        }

        Result<EmployeeId> employeeId = EmployeeId.Create(request.EmployeeId);

        if (employeeId.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), employeeId.Error, true)
                .Warning("Failed to update Staff Member");

            return Result.Failure(employeeId.Error);
        }

        Result<EmailAddress> emailAddress = EmailAddress.Create(request.EmailAddress);

        if (emailAddress.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), emailAddress.Error, true)
                .Warning("Failed to update Staff Member");

            return Result.Failure(emailAddress.Error);
        }

        staffMember.UpdateStaffMember(
            employeeId.Value,
            name.Value,
            emailAddress.Value,
            request.Gender,
            request.IsShared);

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to update staff member");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        staffMember.AddSchoolAssignment(
            request.SchoolCode,
            school.Name,
            _dateTime);
        
        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return Result.Success();
    }
}
