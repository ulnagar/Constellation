namespace Constellation.Application.StaffMembers.CreateStaffMember;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateStaffMemberCommandHandler
: ICommandHandler<CreateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuthService _authService;
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IAuthService authService,
        IOperationService operationService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _authService = authService;
        _operationService = operationService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateStaffMemberCommand>();
    }

    public async Task<Result> Handle(CreateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Staff existing = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (existing is not null)
        {
            _logger
                .ForContext(nameof(CreateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.AlreadyExists(request.StaffId), true)
                .Warning("Failed to create new staff member");

            return Result.Failure(DomainErrors.Partners.Staff.AlreadyExists(request.StaffId));
        }

        Staff staffMember = new()
        {
            StaffId = request.StaffId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PortalUsername = request.PortalUsername,
            SchoolCode = request.SchoolCode,
            IsShared = request.IsShared
        };

        _staffRepository.Insert(staffMember);

        await _unitOfWork.CompleteAsync(cancellationToken);

        await _operationService.CreateTeacherEmployedMSTeamAccess(staffMember.StaffId);

        await _operationService.CreateCanvasUserFromStaff(staffMember);

        UserTemplateDto user = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            IsStaffMember = true,
            StaffId = staffMember.StaffId
        };

        await _authService.CreateUser(user);

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return Result.Success();
    }
}
