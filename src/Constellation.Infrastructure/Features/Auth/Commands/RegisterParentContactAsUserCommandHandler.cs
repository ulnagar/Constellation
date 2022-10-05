namespace Constellation.Infrastructure.Features.Auth.Commands;

using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class RegisterParentContactAsUserCommandHandler : IRequestHandler<RegisterParentContactAsUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<IAuthService> _logger;

    public RegisterParentContactAsUserCommandHandler(UserManager<AppUser> userManager, ILogger<IAuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Unit> Handle(RegisterParentContactAsUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requested to create new user for email {email}", request.EmailAddress);

        var user = await _userManager.FindByEmailAsync(request.EmailAddress);

        if (user != null)
        {
            _logger.LogInformation("Existing user found for email {email}", request.EmailAddress);

            return Unit.Value;
        }

        user = new AppUser
        {
            UserName = request.EmailAddress,
            Email = request.EmailAddress,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        _logger.LogInformation("Attempting to create new user for email {email}", request.EmailAddress);

        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
            _logger.LogInformation("New user created: {email}", request.EmailAddress);
        else
        {
            foreach (var error in result.Errors)
                _logger.LogInformation("New user creation failed for {email} because {error}", request.EmailAddress, error.Description);
        }

        return Unit.Value;
    }
}
