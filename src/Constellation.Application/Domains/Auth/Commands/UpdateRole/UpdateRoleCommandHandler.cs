namespace Constellation.Application.Domains.Auth.Commands.UpdateRole;

using Abstractions.Messaging;
using Application.Models.Identity.Errors;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Threading.Tasks;

internal sealed class UpdateRoleCommandHandler
: ICommandHandler<UpdateRoleCommand, Guid>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateRoleCommandHandler(
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateRoleCommand>();
    }

    public async Task<Result<Guid>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        AppRole? role = await _identityRepository.GetRole(request.Id, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(UpdateRoleCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.RoleNotFound(request.Id), true)
                .Warning("Failed to update Role");

            return Result.Failure<Guid>(AuthErrors.RoleNotFound(request.Id));
        }

        role.Name = request.Name;
        role.UpdateType(request.Type);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return role.Id;
    }
}
