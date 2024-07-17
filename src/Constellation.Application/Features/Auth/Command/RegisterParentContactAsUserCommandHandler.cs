namespace Constellation.Application.Features.Auth.Command;

using Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterParentContactAsUserCommandHandler 
    : IRequestHandler<RegisterParentContactAsUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<IAuthService> _logger;

    public RegisterParentContactAsUserCommandHandler(
        UserManager<AppUser> userManager, 
        ILogger<IAuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(RegisterParentContactAsUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requested to create new user for email {email}", request.EmailAddress);

        AppUser user = await _userManager.FindByEmailAsync(request.EmailAddress);

        if (user != null)
        {
            _logger.LogInformation("Existing user found for email {email}", request.EmailAddress);

            user.IsParent = true;

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                _logger.LogInformation("User updated: {email}", request.EmailAddress);
            else
            {
                foreach (IdentityError error in result.Errors)
                    _logger.LogInformation("User update failed for {email} because {error}", request.EmailAddress, error.Description);
            }
        }
        else
        {
            user = new()
            {
                UserName = request.EmailAddress,
                Email = request.EmailAddress,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsParent = true
            };

            _logger.LogInformation("Attempting to create new user for email {email}", request.EmailAddress);

            IdentityResult result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
                _logger.LogInformation("New user created: {email}", request.EmailAddress);
            else
            {
                foreach (IdentityError error in result.Errors)
                    _logger.LogInformation("New user creation failed for {email} because {error}", request.EmailAddress, error.Description);
            }
        }
    }
}
