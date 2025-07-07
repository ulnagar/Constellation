namespace Constellation.Application.Domains.Casuals.Commands.CreateCasual;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.Casuals;
using Core.Models.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateCasualCommandHandler
    : ICommandHandler<CreateCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateCasualCommandHandler(
        ICasualRepository casualRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _casualRepository = casualRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateCasualCommand>();
    }

    public async Task<Result> Handle(CreateCasualCommand request, CancellationToken cancellationToken)
    {
        Result<Name> nameResult = Name.Create(request.FirstName, string.Empty, request.LastName);
        
        if (nameResult.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateCasualCommand), request, true)
                .ForContext(nameof(Error), nameResult.Error, true)
                .Warning("Failed to create new Casual Teacher for user {User}", _currentUserService.UserName);

            return Result.Failure(nameResult.Error);
        }

        Result<EmailAddress> emailResult = EmailAddress.Create(request.EmailAddress);

        if (emailResult.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateCasualCommand), request, true)
                .ForContext(nameof(Error), emailResult.Error, true)
                .Warning("Failed to create new Casual Teacher for user {User}", _currentUserService.UserName);

            return Result.Failure(emailResult.Error);
        }

        Casual casual = Casual.Create(
            nameResult.Value,
            emailResult.Value,
            request.SchoolCode);

        if (!string.IsNullOrWhiteSpace(request.EdvalTeacherId))
        {
            casual.Update(
                nameResult.Value,
                request.EdvalTeacherId,
                request.SchoolCode);
        }

        _casualRepository.Insert(casual);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
