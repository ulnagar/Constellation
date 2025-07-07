namespace Constellation.Application.Domains.StaffMembers.Commands.CreateStaffMember;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateStaffMemberCommandHandler
    : ICommandHandler<CreateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<CreateStaffMemberCommand>();
    }

    public async Task<Result> Handle(CreateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Result<Name> name = Name.Create(request.FirstName, request.PreferredName, request.LastName);

        if (name.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to create new staff member");

            return Result.Failure(name.Error);
        }

        Result<StaffMember> staffMember = StaffMember.Create(
            name.Value,
            request.Gender,
            request.IsShared);

        if (staffMember.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), staffMember.Error, true)
                .Warning("Failed to create new staff member");

            return Result.Failure(staffMember.Error);
        }

        if (!string.IsNullOrWhiteSpace(request.SchoolCode))
        {
            School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(CreateStaffMemberCommand), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                    .Warning("Failed to create new staff member");

                return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
            }

            staffMember.Value.AddSchoolAssignment(
                request.SchoolCode,
                school.Name,
                _dateTime);
        }

        Result<EmployeeId> employeeId = EmployeeId.Create(request.EmployeeId);

        if (!employeeId.IsFailure)
        {
            StaffMember? existing = await _staffRepository.GetByEmployeeId(employeeId.Value, cancellationToken);

            if (existing is not null)
            {
                _logger
                    .ForContext(nameof(CreateStaffMemberCommand), request, true)
                    .ForContext(nameof(Error), StaffMemberErrors.AlreadyExists(existing.Id), true)
                    .Warning("Failed to create new staff member");

                return Result.Failure(StaffMemberErrors.AlreadyExists(existing.Id));
            }

            Result<EmailAddress> emailAddress = EmailAddress.Create(request.EmailAddress);

            if (emailAddress.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateStaffMemberCommand), request, true)
                    .ForContext(nameof(Error), emailAddress.Error, true)
                    .Warning("Failed to create new staff member");

                return Result.Failure(emailAddress.Error);
            }

            staffMember.Value.UpdateStaffMember(
                employeeId.Value,
                name.Value,
                emailAddress.Value,
                request.Gender,
                request.IsShared);
        }

        _staffRepository.Insert(staffMember.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}