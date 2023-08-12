namespace Constellation.Application.SchoolContacts.RemoveContactRole;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveContactRoleCommandHandler
    : ICommandHandler<RemoveContactRoleCommand>
{
    private readonly ISchoolContactRoleRepository _roleRepository;
    private readonly IAuthService _authService;
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveContactRoleCommandHandler(
        ISchoolContactRoleRepository roleRepository,
        IAuthService authService,
        IOperationService operationService,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _authService = authService;
        _operationService = operationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveContactRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.WithDetails(request.RoleId);

        if (role.SchoolContact.Assignments.Count(assign => !assign.IsDeleted) == 1)
        {
            // This is the last role. User should be updated to remove "IsSchoolContact" flag.
            var newUser = new UserTemplateDto
            {
                FirstName = role.SchoolContact.FirstName,
                LastName = role.SchoolContact.LastName,
                Email = role.SchoolContact.EmailAddress,
                Username = role.SchoolContact.EmailAddress,
                IsSchoolContact = false
            };

            await _authService.UpdateUser(role.SchoolContact.EmailAddress, newUser);

            // Also remove user from the MS Teams
            await _operationService.RemoveContactAddedMSTeamAccess(role.SchoolContactId);
        }

        role.IsDeleted = true;
        role.DateDeleted = DateTime.Now;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
