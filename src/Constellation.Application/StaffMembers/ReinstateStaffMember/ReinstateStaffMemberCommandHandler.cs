namespace Constellation.Application.StaffMembers.ReinstateStaffMember;

using Abstractions.Messaging;
using Application.Models.Identity;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateStaffMemberCommandHandler
: ICommandHandler<ReinstateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IOperationService _operationsService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IOperationService operationsService,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsService = operationsService;
        _userManager = userManager;
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
        UserTemplateDto userDetails = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            IsStaffMember = false
        };

        if (_userManager.Users.Any(u => u.UserName == userDetails.Username))
        {
            AppUser user = await _userManager.FindByEmailAsync(userDetails.Email);

            user!.UserName = userDetails.Username;
            user.Email = userDetails.Email;
            user.FirstName = userDetails.FirstName;
            user.LastName = userDetails.LastName;
            user.IsStaffMember = true;
            user.StaffId = userDetails.StaffId;

            await _userManager.AddToRoleAsync(user, AuthRoles.StaffMember);

            await _userManager.UpdateAsync(user);
        }
        else
        {
            AppUser user = new()
            {
                UserName = userDetails.Username,
                Email = userDetails.Email,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                StaffId = userDetails.StaffId,
                IsSchoolContact = false,
                IsStaffMember = true
            };

            IdentityResult result = await _userManager.CreateAsync(user);

            if (result == IdentityResult.Success)
                await _userManager.AddToRoleAsync(user, AuthRoles.StaffMember);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
