namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMember;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStaffMemberCommandHandler
: ICommandHandler<UpdateStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
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
        
        UserTemplateDto userDetails = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
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

        return Result.Success();
    }
}
