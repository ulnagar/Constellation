namespace Constellation.Application.Domains.Auth.Commands.UpdateUser;

using Abstractions.Messaging;
using Application.Models.Identity.Errors;
using Application.Models.Identity.Repositories;
using Core.Models.Auth;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand>
{
    private readonly ILogger _logger;
    private readonly IIdentityRepository _identityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateUserCommand>();
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUser(request.Id, cancellationToken);

        if (user is null)
        {
            _logger
                .ForContext(nameof(UpdateUserCommand), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(request.Id), true)
                .Warning("Failed to update user");

            return Result.Failure(AuthErrors.UserNotFound(request.Id));
        }

        Result<Name> name = Name.Create(request.FirstName, string.Empty, request.LastName);

        if (name.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateUserCommand), request, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to update user");

            return Result.Failure(name.Error);
        }

        user.Name = name.Value;
        user.Email = request.Email;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
