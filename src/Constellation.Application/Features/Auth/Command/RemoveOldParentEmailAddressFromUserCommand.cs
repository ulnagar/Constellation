namespace Constellation.Application.Features.Auth.Command;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RemoveOldParentEmailAddressFromUserCommand : IRequest
{
    public string Email { get; set; }
}

public class RemoveOldParentEmailAddressFromUserCommandHandler : IRequestHandler<RemoveOldParentEmailAddressFromUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<IAuthService> _logger;

    public RemoveOldParentEmailAddressFromUserCommandHandler(UserManager<AppUser> userManager, ILogger<IAuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveOldParentEmailAddressFromUserCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        _logger.LogInformation("{id}: Requested to remove user {email}", id, request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            _logger.LogInformation("{id}: Found user to remove {email}", id, request.Email);

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("{id}: Successfully removed user {email}", id, request.Email);
            }
            else
            {
                foreach (var error in result.Errors)
                    _logger.LogInformation("{id}: Failed to remove user {email} due to {error}", id, request.Email, error);
            }
        }
        else
        {
            _logger.LogInformation("{id}: Could not find user {email} to remove", id, request.Email);
        }

        return Unit.Value;
    }
}
