namespace Constellation.Infrastructure.Features.Auth.Commands;

using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

public class RegisterParentContactAsUserCommandHandler : IRequestHandler<RegisterParentContactAsUserCommand>
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterParentContactAsUserCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(RegisterParentContactAsUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.EmailAddress);

        if (user != null)
        {
            return Unit.Value;
        }

        user = new AppUser
        {
            UserName = request.EmailAddress,
            Email = request.EmailAddress,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        await _userManager.CreateAsync(user);

        return Unit.Value;
    }
}
