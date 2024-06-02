namespace Constellation.Application.StaffMembers.UpdateStaffMember;

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

internal sealed class UpdateStaffMemberCommandHandler
: ICommandHandler<UpdateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IAuthService authService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _authService = authService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateStaffMemberCommand>();
    }

    public async Task<Result> Handle(UpdateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(UpdateStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to update Staff Member");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        staffMember.FirstName = request.FirstName;
        staffMember.LastName = request.LastName;
        staffMember.PortalUsername = request.PortalUsername;
        staffMember.SchoolCode = request.SchoolCode;
        staffMember.IsShared = request.IsShared;

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        UserTemplateDto newUser = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            StaffId = staffMember.StaffId
        };

        await _authService.UpdateUser(staffMember.EmailAddress, newUser);

        return Result.Success();
    }
}
