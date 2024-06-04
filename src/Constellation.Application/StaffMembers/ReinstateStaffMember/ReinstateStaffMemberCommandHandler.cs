namespace Constellation.Application.StaffMembers.ReinstateStaffMember;

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

internal sealed class ReinstateStaffMemberCommandHandler
: ICommandHandler<ReinstateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IOperationService _operationsService;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IOperationService operationsService,
        IAuthService authService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsService = operationsService;
        _authService = authService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ReinstateStaffMemberCommand>();
    }

    //TODO: R1.15: Refactor these actions to Domain Events in Staff Aggregate
    
    public async Task<Result> Handle(ReinstateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(ReinstateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to reinstate staff member");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        if (!staffMember.IsDeleted)
            return Result.Success();

        staffMember.IsDeleted = false;
        staffMember.DateDeleted = null;

        await _operationsService.CreateTeacherEmployedMSTeamAccess(staffMember.StaffId);

        // Reinstate user access
        UserTemplateDto newUser = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            IsStaffMember = false
        };

        await _authService.UpdateUser(staffMember.EmailAddress, newUser);
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
