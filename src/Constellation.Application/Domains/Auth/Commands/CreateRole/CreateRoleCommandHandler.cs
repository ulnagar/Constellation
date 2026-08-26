namespace Constellation.Application.Domains.Auth.Commands.CreateRole;

using Abstractions.Messaging;
using Application.Models.Identity;
using Application.Models.Identity.Repositories;
using Core.Errors;
using Core.Shared;
using Serilog;
using System;
using System.Threading.Tasks;

internal sealed class CreateRoleCommandHandler
: ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public CreateRoleCommandHandler(
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<CreateRoleCommand>();
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        AppRole role = new(request.Name, request.Type);

        AppRole? result = await _identityRepository.AddRole(role, cancellationToken);

        return result?.Id ?? Result.Failure<Guid>(ApplicationErrors.UnknownError);
    }
}
