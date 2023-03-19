namespace Constellation.Application.Casuals.CreateCasual;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateCasualCommandHandler
    : ICommandHandler<CreateCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCasualCommandHandler(
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateCasualCommand request, CancellationToken cancellationToken)
    {
        var nameResult = Name.Create(request.FirstName, string.Empty, request.LastName);
        
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        var emailResult = EmailAddress.Create(request.EmailAddress);

        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var casual = Casual.Create(
            new CasualId(Guid.NewGuid()),
            nameResult.Value,
            emailResult.Value,
            request.AdobeConnectId,
            request.SchoolCode);

        _casualRepository.Insert(casual);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
