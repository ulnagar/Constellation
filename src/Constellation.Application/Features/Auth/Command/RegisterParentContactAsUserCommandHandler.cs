namespace Constellation.Application.Features.Auth.Command;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterParentContactAsUserCommandHandler 
    : IRequestHandler<RegisterParentContactAsUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RegisterParentContactAsUserCommandHandler(
        UserManager<AppUser> userManager, 
        ILogger logger)
    {
        _userManager = userManager;
        _logger = logger
            .ForContext<RegisterParentContactAsUserCommand>();
    }

    public async Task Handle(RegisterParentContactAsUserCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Requested to create new user for email {email}", request.EmailAddress);

        AppUser user = await _userManager.FindByEmailAsync(request.EmailAddress);

        if (user != null)
        {
            _logger.Information("Existing user found for email {email}", request.EmailAddress);

            user.IsParent = true;

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                _logger.Information("User updated: {email}", request.EmailAddress);
            else
            {
                foreach (IdentityError error in result.Errors)
                    _logger.Information("User update failed for {email} because {error}", request.EmailAddress, error.Description);
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

            _logger.Information("Attempting to create new user for email {email}", request.EmailAddress);

            IdentityResult result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
                _logger.Information("New user created: {email}", request.EmailAddress);
            else
            {
                foreach (IdentityError error in result.Errors)
                    _logger.Information("New user creation failed for {email} because {error}", request.EmailAddress, error.Description);
            }
        }
    }
}
