namespace Constellation.Application.AdminDashboards.RepairSchoolContactUser;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RepairSchoolContactUserCommandHandler
    : ICommandHandler<RepairSchoolContactUserCommand>
{
    private readonly IAuthService _authService;

    public RepairSchoolContactUserCommandHandler(
        IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(RepairSchoolContactUserCommand request, CancellationToken cancellationToken)
    {
        await _authService.RepairSchoolContactUser(request.ContactId);

        return Result.Success();
    }
}
