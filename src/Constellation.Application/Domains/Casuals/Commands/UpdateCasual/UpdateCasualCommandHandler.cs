namespace Constellation.Application.Domains.Casuals.Commands.UpdateCasual;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateCasualCommandHandler
    : ICommandHandler<UpdateCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCasualCommandHandler(
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCasualCommand request, CancellationToken cancellationToken)
    {
        var casual = await _casualRepository.GetById(request.Id, cancellationToken);

        if (casual is null)
        {
            return Result.Failure(DomainErrors.Casuals.Casual.NotFound(request.Id));
        }

        var nameResult = Name.Create(request.FirstName, string.Empty, request.LastName);

        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        casual.Update(
            nameResult.Value,
            request.AdobeConnectId,
            request.SchoolCode);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
