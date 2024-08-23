namespace Constellation.Application.StaffMembers.CreateStaffMember;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
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

internal sealed class CreateStaffMemberCommandHandler
: ICommandHandler<CreateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IOperationService operationService,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationService = operationService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
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

        UserTemplateDto userDetails = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            IsStaffMember = true,
            StaffId = staffMember.StaffId
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
